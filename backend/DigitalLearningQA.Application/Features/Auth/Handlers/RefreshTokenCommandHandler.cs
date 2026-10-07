using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.Auth.Commands;
using DigitalLearningQA.Application.Features.Auth.DTOs;
using DigitalLearningQA.Domain.Entities;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Handlers;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResponse> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var oldToken = await _refreshTokenRepository
            .GetByTokenAsync(request.RefreshToken);

        if (oldToken == null)
            throw new Exception("Refresh token không hợp lệ.");

        if (oldToken.IsRevoked)
            throw new Exception("Refresh token đã bị thu hồi.");

        if (oldToken.ExpiredAt <= DateTime.UtcNow)
            throw new Exception("Refresh token đã hết hạn.");

        // Thu hồi token cũ
        oldToken.IsRevoked = true;

        await _refreshTokenRepository.UpdateAsync(oldToken);

        // Sinh token mới
        var accessToken =
            _jwtService.GenerateAccessToken(oldToken.User);

        var refreshToken =
            _jwtService.GenerateRefreshToken();

        var newToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = oldToken.UserId,

            Token = refreshToken,

            CreatedAt = DateTime.UtcNow,
            ExpiredAt = _jwtService.GetRefreshTokenExpiry(),

            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(newToken);

        return new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}