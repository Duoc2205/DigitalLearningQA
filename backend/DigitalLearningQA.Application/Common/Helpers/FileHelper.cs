using DigitalLearningQA.Application.Common.Constants;

namespace DigitalLearningQA.Application.Common.Helpers;

public static class FileHelper
{
    private static readonly string[] AllowedExtensions =
    {
        ".pdf",
        ".doc",
        ".docx",
        ".ppt",
        ".pptx",
        ".mp4",
        ".zip",
        ".html",
        ".htm"
    };

    public static bool IsAllowedExtension(string extension)
    {
        return AllowedExtensions.Contains(extension.ToLower());
    }

    public static string GetFolder(string extension)
    {
        return extension.ToLower() switch
        {
            ".pdf" => FileFolders.Pdf,

            ".doc" or ".docx" => FileFolders.Word,

            ".ppt" or ".pptx" => FileFolders.PowerPoint,

            ".mp4" => FileFolders.Video,

            _ => FileFolders.Temp
        };
    }

    public static string GetMimeType(string extension) => extension.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".ppt" => "application/vnd.ms-powerpoint",
        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        ".mp4" => "video/mp4",
        ".zip" => "application/zip",
        ".html" or ".htm" => "text/html",
        _ => "application/octet-stream"
    };

    public static async Task<bool> HasValidFileSignatureAsync(Stream stream, string extension, CancellationToken cancellationToken = default)
    {
        if (!stream.CanRead) return false;
        var originalPosition = stream.CanSeek ? stream.Position : 0;
        try
        {
            var buffer = new byte[512];
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(), cancellationToken);
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => bytesRead >= 5 && buffer.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
                ".doc" or ".ppt" => bytesRead >= 8 && buffer.AsSpan(0, 8).SequenceEqual(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }),
                ".docx" or ".pptx" => bytesRead >= 4 && buffer.AsSpan(0, 4).SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
                ".mp4" => bytesRead >= 8 && buffer.AsSpan(4, 4).SequenceEqual("ftyp"u8),
                ".zip" => bytesRead >= 4 && buffer.AsSpan(0, 4).SequenceEqual(new byte[] { 0x50, 0x4B, 0x03, 0x04 }),
                ".html" or ".htm" => IsHtmlDocument(buffer, bytesRead),
                _ => false
            };
        }
        finally { if (stream.CanSeek) stream.Position = originalPosition; }
    }

    private static bool IsHtmlDocument(byte[] buffer, int bytesRead)
    {
        if (bytesRead == 0) return false;
        var text = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead)
            .TrimStart('\uFEFF', ' ', '\t', '\r', '\n')
            .ToLowerInvariant();
        return text.StartsWith("<!doctype html") || text.StartsWith("<html") || text.StartsWith("<head") || text.StartsWith("<body");
    }
}
