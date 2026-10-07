using MediatR;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.CreateTestRequest;

public record CreateTestRequestCommand(
    Guid MaterialVersionId,
    Guid TesterId
) : IRequest<Guid>;