using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class LogoutCommand : IRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}