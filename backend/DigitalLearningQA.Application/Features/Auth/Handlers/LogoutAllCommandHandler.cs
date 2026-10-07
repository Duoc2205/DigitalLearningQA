using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.Auth.Commands;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Handlers;

public class LogoutAllCommandHandler
    : IRequestHandler<LogoutAllCommand>
{
    private readonly IRefreshTokenRepository _refreshRepository;

    public LogoutAllCommandHandler(
        IRefreshTokenRepository refreshRepository)
    {
        _refreshRepository = refreshRepository;
    }

    public async Task Handle(
        LogoutAllCommand request,
        CancellationToken cancellationToken)
    {
        var tokens = await _refreshRepository.GetByUserIdAsync(request.UserId);

        if (tokens.Count == 0)
            return;

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _refreshRepository.UpdateRangeAsync(tokens);
    }
}