namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        Stream stream,
        string fileName,
        string folder,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string filePath);

    bool FileExists(string filePath);
}