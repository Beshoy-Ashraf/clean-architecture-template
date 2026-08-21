using MediatR;
using Application.Common.Interfaces;
using Domain.Exceptions;
using Domain.Interfaces;
using Application.Authentication.Dtos;
using Application.Users.Dtos;



namespace Application.Authentication.Commands.Login;

public class LoginCommandHandler(IUnitOfWork context, ITokenService tokenService, IPasswordHasher hasher) : IRequestHandler<LoginCommand, AuthResponseDto>
{

      public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
      {
            var user = await context.UserRepository.GetUserByEmail(request.Email, cancellationToken)
                ?? throw new UnauthorizedException("Invalid credentials");

            if (!hasher.Verify(request.Password, user.PasswordHash))
                  throw new UnauthorizedException("Invalid credentials");

            var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                  Token = refreshToken,
                  RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            });
            await context.Complete(cancellationToken);

            return new AuthResponseDto(
                accessToken, refreshToken, expiresAt,
                new UserDto(user.Id, user.Username, user.Email, user.DisplayName, user.Role));
      }
}