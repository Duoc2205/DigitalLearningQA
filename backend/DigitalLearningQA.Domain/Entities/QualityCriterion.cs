using DigitalLearningQA.Domain.Common;
using DigitalLearningQA.Domain.Enums;

namespace DigitalLearningQA.Domain.Entities;

public class QualityCriterion : BaseEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public string? Description { get; set; }
    public MaterialType? MaterialType { get; set; }
    public int Weight { get; set; } = 1;
    public bool IsRequired { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
