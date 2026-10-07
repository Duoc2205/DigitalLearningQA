using MediatR;

namespace DigitalLearningQA.Application.Features.AutoTests.Commands.RunAutoTest;

public record RunAutoTestCommand(
    Guid MaterialVersionId
) : IRequest<Guid>;