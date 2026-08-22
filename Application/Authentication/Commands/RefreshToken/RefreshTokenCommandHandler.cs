using System.Security.Claims;
using MediatR;
using Application.Authentication.Dtos;
using Application.Common.Interfaces;
using Application.Users.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IUnitOfWork context, ITokenService tokenService)
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
      public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
      {
            var principal = GetPrincipalOrThrow(request.AccessToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value
                ?? throw new UnauthorizedException("Invalid access token.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                  throw new UnauthorizedException("Invalid access token.");

            var user = await context.UserRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new UnauthorizedException("User not found.");

            var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken)
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (!refreshToken.IsActive)
                  throw new UnauthorizedException("Refresh token is no longer active.");

            refreshToken.IsRevoked = true;

            var (newAccessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var newRefreshTokenValue = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                  Token = newRefreshTokenValue,
                  RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            await context.Complete(cancellationToken);

            return new AuthResponseDto(
                newAccessToken,
                newRefreshTokenValue,
                expiresAt,
                new UserDto(user.Id, user.Username, user.Email, user.DisplayName, user.Role));
      }

      private ClaimsPrincipal GetPrincipalOrThrow(string accessToken)
      {
            try
            {
                  return tokenService.GetPrincipalFromExpiredToken(accessToken)
                      ?? throw new UnauthorizedException("Invalid access token.");
            }
            catch (UnauthorizedException)
            {
                  throw;
            }
            catch (Exception)
            {
                  throw new UnauthorizedException("Invalid access token.");
            }
      }
}