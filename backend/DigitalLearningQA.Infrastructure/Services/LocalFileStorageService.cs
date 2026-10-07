using DigitalLearningQA.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DigitalLearningQA.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var rootPath = configuration["Storage:RootPath"]
            ?? throw new InvalidOperationException("Storage:RootPath is missing.");
        _rootPath = Path.GetFullPath(rootPath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        var directory = GetSafeFullPath(folder);
        Directory.CreateDirectory(directory);

        var newFileName = $"{Guid.NewGuid():N}{Path.GetExtension(fileName)}";
        var fullPath = Path.Combine(directory, newFileName);
        var temporaryPath = $"{fullPath}.{Guid.NewGuid():N}.uploading";
        if (stream.CanSeek) stream.Position = 0;

        try
        {
            await using (var fileStream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                await stream.CopyToAsync(fileStream, cancellationToken);
            File.Move(temporaryPath, fullPath);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }

        return Path.Combine(folder, newFileName).Replace("\\", "/");
    }

    public Task DeleteFileAsync(string filePath)
    {
        var fullPath = GetSafeFullPath(filePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }

    public bool FileExists(string filePath) => File.Exists(GetSafeFullPath(filePath));

    private string GetSafeFullPath(string relativePath)
    {
        var rootWithSeparator = _rootPath.EndsWith(Path.DirectorySeparatorChar) ? _rootPath : _rootPath + Path.DirectorySeparatorChar;
        var fullPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));
        if (!fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("File path is outside the storage root.");
        return fullPath;
    }
}
