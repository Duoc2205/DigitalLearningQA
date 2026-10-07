using MediatR;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetAllTestRequests;

public record GetAllTestRequestsQuery()
    : IRequest<List<TestRequestDto>>;