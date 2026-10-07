namespace DigitalLearningQA.Application.Features.Auth.DTOs;

public class LoginResponse
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public bool IsFirstLogin { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiredAt { get; set; }
}