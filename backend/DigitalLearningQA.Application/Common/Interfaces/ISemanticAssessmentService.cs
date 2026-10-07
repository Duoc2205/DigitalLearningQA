using DigitalLearningQA.Application.Features.AutoTests;

namespace DigitalLearningQA.Application.Common.Interfaces;

/// <summary>Đánh giá ngữ nghĩa từ chính nội dung đã trích xuất của học liệu.</summary>
public interface ISemanticAssessmentService
{
    Task<SemanticAssessmentResult?> AssessAsync(
        string materialType,
        string title,
        string content,
        CancellationToken cancellationToken = default);
}

public sealed class SemanticAssessmentResult
{
    public int ContentScore { get; init; }
    public int PedagogyScore { get; init; }
    public int ObeScore { get; init; }
    public string Opinion { get; init; } = string.Empty;
    public List<AutoAssessmentFinding> Findings { get; init; } = [];
    public List<string> Recommendations { get; init; } = [];
}
