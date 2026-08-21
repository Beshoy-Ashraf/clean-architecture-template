namespace API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Application.Authentication.Commands.Register;
using Application.Authentication.Commands.Login;
using Application.Authentication.Commands.RefreshToken;


[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender mediator) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
          => Ok(await mediator.Send(command));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
        => Ok(await mediator.Send(command));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
        => Ok(await mediator.Send(command));
}