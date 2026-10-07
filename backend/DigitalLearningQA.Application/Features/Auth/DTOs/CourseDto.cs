namespace DigitalLearningQA.Application.Features.Courses.DTOs;

public class CourseDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Credits { get; set; }

    public string Semester { get; set; } = string.Empty;

    public string AcademicYear { get; set; } = string.Empty;

    public Guid DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;

    public Guid? MajorId { get; set; }

    public string? MajorName { get; set; }
}
