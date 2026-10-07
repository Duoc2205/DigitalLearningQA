using System.Security.Claims;
using DigitalLearningQA.Domain.Entities;

namespace DigitalLearningQA.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    DateTime GetRefreshTokenExpiry();

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}