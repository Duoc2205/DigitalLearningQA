using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Queries.GetCLOById;

public class GetCLOByIdQuery : IRequest<CLO?>
{
    public Guid Id { get; set; }
}