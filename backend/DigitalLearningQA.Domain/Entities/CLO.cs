using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class CLO : BaseEntity
{
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    // Ví dụ: CLO1, CLO2...
    public string Code { get; set; } = string.Empty;

    // Mô tả chuẩn đầu ra
    public string Description { get; set; } = string.Empty;

    // Trọng số (nếu có)
    public decimal Weight { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<LearningMaterialCLO> LearningMaterialCLOs
        = new List<LearningMaterialCLO>();
}