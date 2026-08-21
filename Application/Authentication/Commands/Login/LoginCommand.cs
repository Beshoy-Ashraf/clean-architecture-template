using Application.Authentication.Dtos;
using MediatR;

namespace Application.Authentication.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;