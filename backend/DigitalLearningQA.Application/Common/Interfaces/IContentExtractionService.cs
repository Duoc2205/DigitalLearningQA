namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IContentExtractionService
{
    Task<ExtractedContent> ExtractAsync(string relativeFilePath, string extension, CancellationToken cancellationToken = default);
}

public sealed record ExtractedContent(bool IsAvailable, string Text, string? Notice = null);
