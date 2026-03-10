using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QBEngineer.Api.Features.Auth;

namespace QBEngineer.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthUserResponseModel>> Me()
    {
        var result = await mediator.Send(new GetCurrentUserQuery(User));
        return Ok(result);
    }

    [HttpGet("status")]
    [AllowAnonymous]
    public async Task<ActionResult<SetupStatusResponseModel>> Status()
    {
        var result = await mediator.Send(new CheckSetupStatusQuery());
        return Ok(result);
    }

    [HttpPost("setup")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Setup(InitialSetupCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("complete-setup")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> CompleteSetup(CompleteSetupCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("set-pin")]
    [Authorize]
    public async Task<IActionResult> SetPin(SetPinCommand command)
    {
        await mediator.Send(command);
        return NoContent();
    }

    [HttpPost("kiosk-login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> KioskLogin(KioskLoginCommand command)
    {
        var result = await mediator.Send(command);
        return Ok(result);
    }
}
