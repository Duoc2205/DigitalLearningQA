using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class ChecklistReview : BaseEntity
{
    public Guid TestRequestId { get; set; }
    public Guid QualityCriterionId { get; set; }
    // Dat | CanChinhSua | KhongApDung
    public required string Outcome { get; set; }
    public string? Note { get; set; }
    public Guid ReviewedById { get; set; }
    public DateTime ReviewedAt { get; set; }
    public TestRequest TestRequest { get; set; } = null!;
    public QualityCriterion QualityCriterion { get; set; } = null!;
    public User ReviewedBy { get; set; } = null!;
}
