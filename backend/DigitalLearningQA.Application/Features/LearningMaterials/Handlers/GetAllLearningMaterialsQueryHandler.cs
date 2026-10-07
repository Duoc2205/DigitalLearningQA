using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.LearningMaterials.DTOs;
using DigitalLearningQA.Application.Features.LearningMaterials.Queries;
using MediatR;

namespace DigitalLearningQA.Application.Features.LearningMaterials.Handlers;

public class GetAllLearningMaterialsQueryHandler
    : IRequestHandler<GetAllLearningMaterialsQuery, List<LearningMaterialDto>>
{
    private readonly ILearningMaterialRepository _repository;

    public GetAllLearningMaterialsQueryHandler(
        ILearningMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<LearningMaterialDto>> Handle(
        GetAllLearningMaterialsQuery request,
        CancellationToken cancellationToken)
    {
        var materials = await _repository.GetAllAsync();

        return materials.Select(x => new LearningMaterialDto
        {
            Id = x.Id,
            Code = x.Code,
            Title = x.Title,
            Description = x.Description,

            CourseId = x.CourseId,
            CourseName = x.Course.Name,

            OwnerId = x.OwnerId,
            OwnerName = x.Owner.FullName,

            MaterialType = (int)x.MaterialType,
            Status = (int)x.Status,

            CurrentVersionId = x.CurrentVersionId
        }).ToList();
    }
}