using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.Courses.DTOs;
using DigitalLearningQA.Application.Features.Courses.Queries;
using MediatR;

namespace DigitalLearningQA.Application.Features.Courses.Handlers;

public class GetAllCoursesQueryHandler
    : IRequestHandler<GetAllCoursesQuery, List<CourseDto>>
{
    private readonly ICourseRepository _repository;

    public GetAllCoursesQueryHandler(ICourseRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CourseDto>> Handle(
        GetAllCoursesQuery request,
        CancellationToken cancellationToken)
    {
        var courses = await _repository.GetAllAsync();

        return courses.Select(x => new CourseDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            Credits = x.Credits,
            Semester = x.Semester,
            AcademicYear = x.AcademicYear,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department.Name,
            MajorId = x.MajorId,
            MajorName = x.Major != null ? x.Major.Name : null
        }).ToList();
    }
}
