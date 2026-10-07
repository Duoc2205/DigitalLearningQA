using DigitalLearningQA.Domain.Common;

namespace DigitalLearningQA.Domain.Entities;

public class Department : BaseEntity
{
    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Course> Courses { get; set; } = new List<Course>();

    public ICollection<Major> Majors { get; set; } = new List<Major>();

    public ICollection<User> Users { get; set; } = new List<User>();
}
