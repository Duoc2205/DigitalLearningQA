using MediatR;

namespace DigitalLearningQA.Application.Features.LearningMaterials.Commands;

public record CreateLearningMaterialCommand(
    string Code,
    Guid CourseId,
    Guid OwnerId,
    string Title,
    string? Description,
    int MaterialType
) : IRequest<Guid>;