using System.Security.Claims;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ITokenService
{
      (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user);
      string GenerateRefreshToken();
      ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
