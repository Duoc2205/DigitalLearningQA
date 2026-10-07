
using MediatR;

namespace DigitalLearningQA.Application.Features.Courses.Commands;

public record CreateCourseCommand(
    Guid DepartmentId,
    Guid? MajorId,
    string Code,
    string Name,
    string? Description,
    int Credits,
    string Semester,
    string AcademicYear
) : IRequest<Guid>;
