using DigitalLearningQA.Application.Features.Courses.DTOs;
using MediatR;

namespace DigitalLearningQA.Application.Features.Courses.Queries;

public record GetAllCoursesQuery()
    : IRequest<List<CourseDto>>;