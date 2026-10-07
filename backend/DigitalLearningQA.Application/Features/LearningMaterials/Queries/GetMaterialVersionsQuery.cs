using MediatR;

namespace DigitalLearningQA.Application.Features.MaterialVersions.Queries;

public class GetMaterialVersionsQuery : IRequest<List<MaterialVersionDto>>
{
    public Guid LearningMaterialId { get; set; }
}