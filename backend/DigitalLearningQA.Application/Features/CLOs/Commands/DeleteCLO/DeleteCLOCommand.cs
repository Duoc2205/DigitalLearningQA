using DigitalLearningQA.Application.Common.Responses;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Commands.DeleteCLO;

public class DeleteCLOCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}