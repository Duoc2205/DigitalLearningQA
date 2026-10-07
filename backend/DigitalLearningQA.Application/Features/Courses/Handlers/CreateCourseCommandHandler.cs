using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Features.Courses.Commands;
using DigitalLearningQA.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.Courses.Handlers;

public class CreateCourseCommandHandler
    : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly ICourseRepository _repository;
    private readonly IApplicationDbContext _db;

    public CreateCourseCommandHandler(ICourseRepository repository, IApplicationDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    public async Task<Guid> Handle(
        CreateCourseCommand request,
        CancellationToken cancellationToken)
    {
        if (!await _db.Departments.AnyAsync(x => x.Id == request.DepartmentId && x.IsActive, cancellationToken))
            throw new ResourceNotFoundException("Khoa/đơn vị không hợp lệ.");
        if (request.MajorId.HasValue && !await _db.Majors.AnyAsync(x => x.Id == request.MajorId && x.DepartmentId == request.DepartmentId && x.IsActive, cancellationToken))
            throw new RequestValidationException("Ngành không thuộc khoa đã chọn.");
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            throw new RequestValidationException("Mã và tên học phần là bắt buộc.");
        if (request.Credits < 1 || request.Credits > 20)
            throw new RequestValidationException("Số tín chỉ phải từ 1 đến 20.");
        if (await _db.Courses.AnyAsync(x => x.DepartmentId == request.DepartmentId && x.Code == request.Code.Trim(), cancellationToken))
            throw new ConflictException("Mã học phần đã tồn tại trong khoa này.");

        var course = new Course
        {
            DepartmentId = request.DepartmentId,
            MajorId = request.MajorId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Credits = request.Credits,
            Semester = request.Semester,
            AcademicYear = request.AcademicYear
        };

        await _repository.AddAsync(course);

        return course.Id;
    }
}
