using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Queries.GetCLOById;

public class GetCLOByIdHandler
    : IRequestHandler<GetCLOByIdQuery, CLO?>
{
    private readonly ICLORepository _repository;

    public GetCLOByIdHandler(ICLORepository repository)
    {
        _repository = repository;
    }

    public async Task<CLO?> Handle(
        GetCLOByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.Id);
    }
}   