using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.CLOs.Queries.GetAllCLOs;

public class GetAllCLOHandler
    : IRequestHandler<GetAllCLOQuery, List<CLO>>
{
    private readonly ICLORepository _repository;

    public GetAllCLOHandler(ICLORepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CLO>> Handle(
        GetAllCLOQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync();
    }
}