using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DigitalLearningQA.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalLearningQA.Infrastructure.Services;

/// <summary>Trích xuất văn bản thực từ các loại học liệu được hệ thống hỗ trợ.</summary>
public sealed class ContentExtractionService : IContentExtractionService
{
    private const int MaxExtractedCharacters = 100_000;
    private readonly string _rootPath;

    public ContentExtractionService(IConfiguration configuration)
    {
        _rootPath = Path.GetFullPath(configuration["Storage:RootPath"]
            ?? throw new InvalidOperationException("Storage:RootPath is missing."));
    }

    public async Task<ExtractedContent> ExtractAsync(string relativeFilePath, string extension, CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafeFullPath(relativeFilePath);
        if (!File.Exists(fullPath)) return new(false, string.Empty, "Không tìm thấy tệp đã tải lên để đọc nội dung.");
        try
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => await ExtractPdfAsync(fullPath, cancellationToken),
                ".docx" => await ExtractOfficeAsync(fullPath, "word/document.xml", cancellationToken),
                ".pptx" => await ExtractPowerPointAsync(fullPath, cancellationToken),
                ".html" or ".htm" => CleanResult(await File.ReadAllTextAsync(fullPath, cancellationToken), "Trang HTML không có văn bản để trích xuất.", true),
                ".zip" => await ExtractScormAsync(fullPath, cancellationToken),
                ".doc" => new(false, string.Empty, "Tệp Word .doc cũ chưa đọc được trực tiếp; hãy lưu thành .docx hoặc PDF."),
                ".ppt" => new(false, string.Empty, "Tệp PowerPoint .ppt cũ chưa đọc được trực tiếp; hãy lưu thành .pptx hoặc PDF."),
                ".mp4" => new(false, string.Empty, "Video không có phụ đề hoặc kịch bản văn bản để trích xuất. Hãy nộp kèm transcript trong mô tả hoặc tệp PDF."),
                _ => new(false, string.Empty, "Định dạng tệp này chưa hỗ trợ trích xuất nội dung.")
            };
        }
        catch (InvalidDataException) { return new(false, string.Empty, "Tệp nén/Office không hợp lệ hoặc bị hỏng."); }
    }

    private static async Task<ExtractedContent> ExtractPdfAsync(string fullPath, CancellationToken cancellationToken)
    {
        try
        {
            var info = new ProcessStartInfo
            {
                FileName = "pdftotext", Arguments = $"-enc UTF-8 -layout \"{fullPath}\" -",
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
            };
            using var process = Process.Start(info);
            if (process is null) return new(false, string.Empty, "Không thể khởi tạo bộ trích xuất PDF.");
            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);
            var text = await outputTask;
            var error = await errorTask;
            if (process.ExitCode != 0) return new(false, string.Empty, string.IsNullOrWhiteSpace(error) ? "Không thể đọc văn bản trong PDF." : error);
            var extracted = CleanResult(text, "PDF không có lớp văn bản có thể trích xuất; có thể đây là bản quét ảnh và cần OCR.");
            return extracted.IsAvailable ? extracted : await TryOcrPdfAsync(fullPath, cancellationToken);
        }
        catch (Exception exception) when (exception is Win32Exception or FileNotFoundException)
        {
            return new(false, string.Empty, "Máy chủ chưa cài bộ trích xuất PDF pdftotext.");
        }
    }

    private static async Task<ExtractedContent> ExtractOfficeAsync(string fullPath, string entryName, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(fullPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var entry = archive.GetEntry(entryName);
        if (entry is null) return new(false, string.Empty, "Không tìm thấy nội dung văn bản trong tệp Office.");
        await using var entryStream = entry.Open();
        using var reader = new StreamReader(entryStream, Encoding.UTF8, true);
        return CleanResult(await reader.ReadToEndAsync(cancellationToken), "Tệp Office không có văn bản để trích xuất.", true);
    }

    private static async Task<ExtractedContent> TryOcrPdfAsync(string fullPath, CancellationToken cancellationToken)
    {
        var folder = Path.Combine(Path.GetTempPath(), "DigitalLearningQA-Ocr-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            var prefix = Path.Combine(folder, "page");
            var render = await RunProcessAsync("pdftoppm", $"-f 1 -l 10 -r 180 -png \"{fullPath}\" \"{prefix}\"", cancellationToken);
            if (render.ExitCode != 0) return new(false, string.Empty, "PDF không có lớp chữ và không thể chuyển trang để OCR.");
            var pages = Directory.GetFiles(folder, "page-*.png").OrderBy(x => x).ToList();
            if (pages.Count == 0) return new(false, string.Empty, "PDF không có lớp chữ và không có trang nào để OCR.");
            var text = new StringBuilder();
            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var ocr = await RunProcessAsync("tesseract", $"\"{page}\" stdout -l vie+eng", cancellationToken);
                if (ocr.ExitCode == 0) text.AppendLine(ocr.Output);
            }
            var result = CleanResult(text.ToString(), "PDF quét ảnh chưa OCR được. Hãy cài Tesseract OCR kèm ngôn ngữ Việt trên máy chủ hoặc nộp PDF có lớp chữ.");
            return result.IsAvailable ? new(true, result.Text, "Nội dung được trích xuất bằng OCR từ PDF quét ảnh.") : result;
        }
        catch (Exception exception) when (exception is Win32Exception or FileNotFoundException)
        {
            return new(false, string.Empty, "PDF quét ảnh cần Tesseract OCR kèm ngôn ngữ Việt trên máy chủ để đọc nội dung.");
        }
        finally
        {
            if (Directory.Exists(folder)) Directory.Delete(folder, true);
        }
    }

    private static async Task<(int ExitCode, string Output)> RunProcessAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        var info = new ProcessStartInfo { FileName = fileName, Arguments = arguments, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        using var process = Process.Start(info) ?? throw new FileNotFoundException(fileName);
        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, await outputTask);
    }

    private static async Task<ExtractedContent> ExtractPowerPointAsync(string fullPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(fullPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var slides = archive.Entries.Where(x => Regex.IsMatch(x.FullName, @"^ppt/slides/slide\d+\.xml$", RegexOptions.IgnoreCase)).OrderBy(x => x.FullName).ToList();
        if (slides.Count == 0) return new(false, string.Empty, "Không tìm thấy nội dung slide trong PowerPoint.");
        var text = new StringBuilder();
        foreach (var slide in slides)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var entryStream = slide.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, true);
            text.AppendLine(await reader.ReadToEndAsync(cancellationToken));
        }
        return CleanResult(text.ToString(), "PowerPoint không có văn bản để trích xuất.", true);
    }

    private static async Task<ExtractedContent> ExtractScormAsync(string fullPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(fullPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        var pages = archive.Entries.Where(x => x.Length > 0 && (x.FullName.Equals("imsmanifest.xml", StringComparison.OrdinalIgnoreCase) || x.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || x.FullName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))).Take(30).ToList();
        if (pages.Count == 0) return new(false, string.Empty, "Gói SCORM không có imsmanifest.xml hoặc trang HTML để trích xuất nội dung.");
        var text = new StringBuilder();
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var entryStream = page.Open();
            using var reader = new StreamReader(entryStream, Encoding.UTF8, true);
            text.AppendLine(await reader.ReadToEndAsync(cancellationToken));
        }
        return CleanResult(text.ToString(), "Gói SCORM chưa có văn bản đủ để trích xuất.", true);
    }

    private static ExtractedContent CleanResult(string raw, string emptyNotice, bool containsMarkup = false)
    {
        var text = containsMarkup ? Regex.Replace(raw, @"<[^>]+>", " ") : raw;
        text = Regex.Replace(WebUtility.HtmlDecode(text), @"\s+", " ").Trim();
        if (text.Length > MaxExtractedCharacters) text = text[..MaxExtractedCharacters];
        return string.IsNullOrWhiteSpace(text) ? new(false, string.Empty, emptyNotice) : new(true, text);
    }

    private string GetSafeFullPath(string relativePath)
    {
        var root = _rootPath.EndsWith(Path.DirectorySeparatorChar) ? _rootPath : _rootPath + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Đường dẫn tệp không hợp lệ.");
        return full;
    }
}
