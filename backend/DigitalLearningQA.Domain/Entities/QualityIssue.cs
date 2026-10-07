using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class QualityIssue : BaseEntity
{
    public Guid TestRequestId { get; set; }
    public required string Title { get; set; }
    public required string Category { get; set; }
    // Thap | TrungBinh | Cao | NghiemTrong
    public required string Severity { get; set; }
    public required string Description { get; set; }
    public string Status { get; set; } = "Mo";
    public string? EvidenceUrl { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? ResolvedById { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? ResolutionNote { get; set; }
    public TestRequest TestRequest { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
    public User? ResolvedByUser { get; set; }
}
