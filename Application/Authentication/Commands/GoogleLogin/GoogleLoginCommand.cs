using Application.Authentication.Dtos;
using MediatR;

namespace Application.Authentication.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<AuthResponseDto>;
