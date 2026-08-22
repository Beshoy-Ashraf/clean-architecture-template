using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Application.Common.Interfaces;
using Domain.Entities;
namespace Infrastructure.Identity;

public class TokenService(IOptions<JwtSettings> settings) : ITokenService
{
      private readonly JwtSettings _settings = settings.Value;

      public (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(User user)
      {
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
      }
      public string GenerateRefreshToken()
      {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
      }

      public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
      {
            var validation = new TokenValidationParameters
            {
                  ValidateAudience = false,
                  ValidateIssuer = false,
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey)),
                  ValidateLifetime = false
            };

            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, validation, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                  throw new SecurityTokenException("Invalid token");
            }

            return principal;
      }
}