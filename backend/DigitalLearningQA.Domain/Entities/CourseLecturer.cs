namespace DigitalLearningQA.Domain.Entities;

public class CourseLecturer
{
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;
}