using DigitalLearningQA.Application.Features.LearningMaterials.DTOs;
using MediatR;

namespace DigitalLearningQA.Application.Features.LearningMaterials.Queries;

public record GetAllLearningMaterialsQuery()
    : IRequest<List<LearningMaterialDto>>;