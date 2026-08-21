// Application/Auth/Commands/Register/RegisterCommandHandler.cs
using MediatR;
using Application.Common.Interfaces;
using Application.Users.Dtos;
using Domain.Entities;
using Domain.Exceptions;
using Domain.Interfaces;
using Application.Authentication.Dtos;
using Application.Authentication.Commands.Register;

namespace Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    IUnitOfWork context,
    ITokenService tokenService,
    IPasswordHasher hasher) : IRequestHandler<RegisterCommand, AuthResponseDto>
{

      public async Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
      {
            var existingEmail = await context.UserRepository.GetUserByEmail(request.Email, cancellationToken);
            if (existingEmail is not null)
                  throw new ConflictException($"Email '{request.Email}' is already registered.");

            var existingUsername = await context.UserRepository.GetUserByUsername(request.Username, cancellationToken);
            if (existingUsername is not null)
                  throw new ConflictException($"Username '{request.Username}' is already taken.");

            var passwordHash = hasher.Hash(request.Password);

            var user = new User(
                request.Username,
                request.Email,
                passwordHash,
                request.DisplayName,
                request.ProfilePictureUrl,
                role: "User");

            await context.UserRepository.AddAsync(user, cancellationToken);

            var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            user.RefreshTokens.Add(new Domain.Entities.RefreshToken
            {
                  Token = refreshToken,
                  RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7)
            });

            await context.Complete(cancellationToken);

            return new AuthResponseDto(
                accessToken,
                refreshToken,
                expiresAt,
                new UserDto(user.Id, user.Username, user.Email, user.DisplayName, user.Role));
      }
}