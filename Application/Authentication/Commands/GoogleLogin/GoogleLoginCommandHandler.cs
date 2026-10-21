using MediatR;
using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Application.Authentication.Dtos;
using Application.Users.Dtos;

namespace Application.Authentication.Commands.GoogleLogin;

public class GoogleLoginCommandHandler(
    IUnitOfWork context,
    IGoogleAuthService googleAuthService,
    ITokenService tokenService) : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{

      public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
      {
            var payload = await googleAuthService.ValidateAsync(request.IdToken, cancellationToken);

            if (!payload.EmailVerified)
                  throw new UnauthorizedException("Google account email is not verified");

            var user = await context.UserRepository.GetUserByEmail(payload.Email, cancellationToken);

            if (user is null)
            {
                  var username = await GenerateUniqueUsername(payload.Email, cancellationToken);

                  user = new User(
                      username,
                      payload.Email,
                      passwordHash: string.Empty,
                      displayName: string.IsNullOrWhiteSpace(payload.Name) ? username : payload.Name,
                      profilePictureUrl: payload.PictureUrl,
                      role: "User");

                  await context.UserRepository.AddAsync(user, cancellationToken);
            }

            var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                  Token = refreshToken,
                  RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            await context.Complete(cancellationToken);

            return new AuthResponseDto(
                accessToken, refreshToken, expiresAt,
                new UserDto(user.Id, user.Username, user.Email, user.DisplayName, user.Role));
      }

      private async Task<string> GenerateUniqueUsername(string email, CancellationToken cancellationToken)
      {
            var baseUsername = email.Split('@')[0];
            var username = baseUsername;
            var suffix = 0;

            while (await context.UserRepository.GetUserByUsername(username, cancellationToken) is not null)
                  username = $"{baseUsername}{++suffix}";

            return username;
      }
}
