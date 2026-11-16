using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.Features.Authentication.Register;
using SupportPilot.Application.Features.Authentication.Login;

namespace SupportPilot.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly RegisterUserService _registerUserService;
    private readonly LoginUserService _loginUserService;

    public AuthenticationController(
        RegisterUserService registerUserService,
        LoginUserService loginUserService)
    {
        _registerUserService = registerUserService;
        _loginUserService = loginUserService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerUserService.ExecuteAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                id = result.Value,
                message = "User registered successfully."
            });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _loginUserService.ExecuteAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }
}