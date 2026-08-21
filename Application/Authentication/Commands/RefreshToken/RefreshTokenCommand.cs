using Application.Authentication.Dtos;
using MediatR;

namespace Application.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;