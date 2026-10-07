using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.Auth.Commands;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Handlers;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshRepository;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshRepository)
    {
        _refreshRepository = refreshRepository;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var token =
            await _refreshRepository.GetByTokenAsync(request.RefreshToken);

        if (token == null)
            return;

        token.IsRevoked = true;

        await _refreshRepository.UpdateAsync(token);
    }
}