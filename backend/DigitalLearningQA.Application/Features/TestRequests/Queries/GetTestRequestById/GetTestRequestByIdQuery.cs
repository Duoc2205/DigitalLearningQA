using MediatR;

namespace DigitalLearningQA.Application.Features.TestRequests.Queries.GetTestRequestById;

public record GetTestRequestByIdQuery(Guid Id)
    : IRequest<TestRequestDetailDto>;