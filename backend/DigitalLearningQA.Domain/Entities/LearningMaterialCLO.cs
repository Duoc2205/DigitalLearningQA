using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class LearningMaterialCLO : BaseEntity
{
    public Guid LearningMaterialId { get; set; }

    public LearningMaterial LearningMaterial { get; set; } = null!;

    public Guid CLOId { get; set; }

    public CLO CLO { get; set; } = null!;
}