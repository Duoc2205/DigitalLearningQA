using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class LogoutAllCommand : IRequest
{
    public Guid UserId { get; set; }
}