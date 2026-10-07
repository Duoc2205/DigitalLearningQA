using MediatR;

namespace DigitalLearningQA.Application.Features.TestRequests.Commands.StartTestRequest;

public record StartTestRequestCommand(Guid Id) : IRequest;