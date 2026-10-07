using DigitalLearningQA.Application.Common.Interfaces;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Commands;

public class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);

        if (user is null)
            throw new Exception("User not found.");

        var valid = _passwordHasher.VerifyPassword(
            request.OldPassword,
            user.PasswordHash);

        if (!valid)
            throw new Exception("Old password is incorrect.");

        user.PasswordHash =
            _passwordHasher.HashPassword(request.NewPassword);

        user.IsFirstLogin = false;

        await _userRepository.UpdateAsync(user);

        return true;
    }
}