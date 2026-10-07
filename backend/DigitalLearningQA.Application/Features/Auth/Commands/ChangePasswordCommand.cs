using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class ChangePasswordCommand : IRequest<bool>
{
    public Guid UserId { get; set; }

    public string OldPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}