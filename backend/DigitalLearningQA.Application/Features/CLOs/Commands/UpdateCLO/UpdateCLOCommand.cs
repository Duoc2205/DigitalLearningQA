using DigitalLearningQA.Application.Common.Responses;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.UpdateCLO;

public class UpdateCLOCommand : IRequest<ApiResponse<Guid>>
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}