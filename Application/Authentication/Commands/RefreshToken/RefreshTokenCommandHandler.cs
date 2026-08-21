using System.Security.Claims;
using MediatR;
using Application.Authentication.Dtos;
using Application.Common.Interfaces;
using Application.Users.Dtos;
using Domain.Exceptions;
using Domain.Interfaces;

namespace Application.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IUnitOfWork context, ITokenService tokenService) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{

      public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
      {
            ClaimsPrincipal principal;
            try
            {
                  principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken)
                      ?? throw new UnauthorizedException("Invalid access token.");
            }
            catch (Exception)
            {
                  throw new UnauthorizedException("Invalid access token.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst("sub")?.Value
                ?? throw new UnauthorizedException("Invalid access token.");

            if (!Guid.TryParse(userIdClaim, out var userId))
                  throw new UnauthorizedException("Invalid access token.");

            var user = await context.UserRepository.GetUserByUserId(userId, cancellationToken)
                ?? throw new UnauthorizedException("User not found.");

            var refreshToken = user.RefreshTokens.FirstOrDefault(rt => rt.Token == request.RefreshToken)
                ?? throw new UnauthorizedException("Invalid refresh token.");

            if (refreshToken.RefreshTokenExpiryTime is null || refreshToken.RefreshTokenExpiryTime <= DateTime.UtcNow)
                  throw new UnauthorizedException("Refresh token has expired.");

            var (newAccessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var newRefreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Remove(refreshToken);
            user.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                  Token = newRefreshToken,
                  RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            await context.Complete(cancellationToken);

            return new AuthResponseDto(
                newAccessToken,
                newRefreshToken,
                expiresAt,
                new UserDto(user.Id, user.Username, user.Email, user.DisplayName, user.Role));
      }
}