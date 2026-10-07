using DigitalLearningQA.Application.Common.Responses;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.CreateCLO;

public class CreateCLOCommand : IRequest<ApiResponse<Guid>>
{
    public Guid CourseId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}