using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class Course : BaseEntity
{
    public Guid DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public Guid? MajorId { get; set; }

    public Major? Major { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public int Credits { get; set; }

    public string Semester { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<CourseLecturer> CourseLecturers { get; set; }
        = new List<CourseLecturer>();

    public ICollection<LearningMaterial> LearningMaterials { get; set; }
        = new List<LearningMaterial>();

    public ICollection<CLO> CLOs { get; set; }
        = new List<CLO>();
}
