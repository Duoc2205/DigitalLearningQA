using System.Security.Claims;
using DigitalLearningQA.API.DTOs.Auth;
using DigitalLearningQA.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalLearningQA.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _mediator.Send(
                new LoginCommand
                {
                    Username = request.Username,
                    Password = request.Password
                });

            return Ok(result);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Làm mới Access Token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequest request)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken
            });

        return Ok(result);
    }

    /// <summary>
    /// Đăng xuất
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        LogoutRequest request)
    {
        await _mediator.Send(
            new LogoutCommand
            {
                RefreshToken = request.RefreshToken
            });

        return Ok(new
        {
            Message = "Logout successfully."
        });
    }

    /// <summary>
    /// Đăng xuất tất cả thiết bị
    /// </summary>
    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _mediator.Send(
            new LogoutAllCommand
            {
                UserId = userId
            });

        return Ok(new
        {
            Message = "Logout all devices successfully."
        });
    }

    /// <summary>
    /// Lấy thông tin người dùng đang đăng nhập
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Username = User.FindFirstValue(ClaimTypes.Name),
            FullName = User.FindFirstValue(ClaimTypes.GivenName),
            Email = User.FindFirstValue(ClaimTypes.Email),
            Role = User.FindFirstValue(ClaimTypes.Role)
        });
    }

    /// <summary>
    /// Đổi mật khẩu
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request)
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });

        try
        {
            await _mediator.Send(
                new ChangePasswordCommand
                {
                    UserId = userId,
                    OldPassword = request.OldPassword,
                    NewPassword = request.NewPassword
                });
        }
        catch (Exception exception)
        {
            return BadRequest(new { message = exception.Message });
        }

        return Ok(new
        {
            Message = "Đổi mật khẩu thành công."
        });
    }
}
