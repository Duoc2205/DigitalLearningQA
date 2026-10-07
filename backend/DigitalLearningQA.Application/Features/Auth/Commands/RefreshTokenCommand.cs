using DigitalLearningQA.Application.Features.Auth.DTOs;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class RefreshTokenCommand
    : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}