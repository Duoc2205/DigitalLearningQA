using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class Major : BaseEntity
{
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public required string Code { get; set; }
    public required string Name { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Course> Courses { get; set; } = new List<Course>();
}
