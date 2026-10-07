using DigitalLearningQA.Application.Common.Interfaces;
using DigitalLearningQA.Application.Features.Auth.Commands;
using DigitalLearningQA.Application.Features.Auth.DTOs;
using DigitalLearningQA.Domain.Entities;
using DigitalLearningQA.Domain.Enums;
using MediatR;

namespace DigitalLearningQA.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogRepository auditLogRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<LoginResponse> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Kiểm tra tài khoản
        var user = await _userRepository.GetByUsernameAsync(request.Username);

        if (user == null)
            throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu.");

        // Kiểm tra mật khẩu
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu.");

        // Kiểm tra trạng thái tài khoản
        if (user.Status != UserStatus.Active)
            throw new UnauthorizedAccessException("Tài khoản đã bị khóa.");

        // Chỉ giữ 1 RefreshToken cho mỗi user
        await _refreshTokenRepository.DeleteByUserIdAsync(user.Id);

        // Sinh AccessToken
        var accessToken = _jwtService.GenerateAccessToken(user);

        // Sinh RefreshToken
        var refreshToken = _jwtService.GenerateRefreshToken();

        var refresh = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiredAt = _jwtService.GetRefreshTokenExpiry(),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _refreshTokenRepository.AddAsync(refresh);

        // Audit Log
        await _auditLogRepository.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Action = "LOGIN",
            Description = $"{user.Username} đăng nhập hệ thống.",
            CreatedAt = DateTime.UtcNow
        });

        // Trả kết quả
        return new LoginResponse
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Role = user.Role.Name,
            Department = user.Department.Name,

            IsFirstLogin = user.IsFirstLogin,

            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiredAt = refresh.ExpiredAt
        };
    }
}