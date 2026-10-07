using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Common.Exceptions;
using DigitalLearningQA.Application.Features.LearningMaterials.Commands;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DigitalLearningQA.Application.Features.LearningMaterials.Handlers;

public class CreateLearningMaterialCommandHandler
    : IRequestHandler<CreateLearningMaterialCommand, Guid>
{
    private readonly ILearningMaterialRepository _repository;
    private readonly IApplicationDbContext _db;

    public CreateLearningMaterialCommandHandler(
        ILearningMaterialRepository repository,
        IApplicationDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    public async Task<Guid> Handle(
        CreateLearningMaterialCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Title))
            throw new RequestValidationException("Mã và tên học liệu là bắt buộc.");
        if (!Enum.IsDefined(typeof(MaterialType), request.MaterialType))
            throw new RequestValidationException("Loại học liệu không hợp lệ.");
        if (!await _db.Courses.AnyAsync(x => x.Id == request.CourseId && x.IsActive, cancellationToken))
            throw new ResourceNotFoundException("Học phần không tồn tại hoặc đã ngừng sử dụng.");
        if (await _db.LearningMaterials.AnyAsync(x => x.Code == request.Code, cancellationToken))
            throw new ConflictException("Mã học liệu đã tồn tại.");
        var material = new LearningMaterial
        {
            Code = request.Code,
            CourseId = request.CourseId,
            OwnerId = request.OwnerId,
            Title = request.Title,
            Description = request.Description,
            MaterialType = (MaterialType)request.MaterialType,
            Status = MaterialStatus.Draft
        };

        await _repository.AddAsync(material);

        return material.Id;
    }
}
