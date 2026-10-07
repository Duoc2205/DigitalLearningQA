using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetAllTestRequests;

public class TestRequestDto
{
    public Guid Id { get; set; }

    // Learning Material
    public Guid LearningMaterialId { get; set; }
    public string LearningMaterialTitle { get; set; } = string.Empty;

    // Material Version
    public Guid MaterialVersionId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;

    // Người upload phiên bản
    public Guid UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;

    // Người kiểm thử
    public Guid? TesterId { get; set; }
    public string? TesterName { get; set; }

    // Dates
    public DateTime RequestedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    // Status
    public TestRequestStatus Status { get; set; }

    public string? Conclusion { get; set; }
}
