using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetTestRequestById;

public class TestRequestDetailDto
{
    public Guid Id { get; set; }

    public Guid LearningMaterialId { get; set; }
    public string LearningMaterialTitle { get; set; } = string.Empty;

    public Guid MaterialVersionId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;

    public Guid UploadedBy { get; set; }
    public string UploadedByName { get; set; } = string.Empty;

    public Guid? TesterId { get; set; }
    public string? TesterName { get; set; }

    public DateTime RequestedDate { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public TestRequestStatus Status { get; set; }

    public string? Conclusion { get; set; }
}
