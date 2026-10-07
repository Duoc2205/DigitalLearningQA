using DigitalLearningQA.Application.Features.Auth.DTOs;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public LoginCommand()
    {
    }

    public LoginCommand(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}