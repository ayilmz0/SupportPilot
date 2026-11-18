
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Features.Authentication.Login;
using SupportPilot.Application.Features.Authentication.Refresh;
using SupportPilot.Application.Features.Authentication.Register;
using SupportPilot.Application.Features.Authentication.Logout;
using SupportPilot.Infrastructure.Security;

namespace SupportPilot.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly RegisterUserService _registerUserService;
    private readonly LoginUserService _loginUserService;
    private readonly JwtSettings _jwtSettings;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly LogoutUserService _logoutUserService;
    private readonly IRedisService _redisService;

    public AuthenticationController(
        RegisterUserService registerUserService,
        LoginUserService loginUserService,
        RefreshTokenService refreshTokenService,
        LogoutUserService logoutUserService,
        IOptions<JwtSettings> jwtOptions,
        IRedisService redisService)
    {
        _registerUserService = registerUserService;
        _loginUserService = loginUserService;
        _refreshTokenService = refreshTokenService;
        _logoutUserService = logoutUserService;
        _jwtSettings = jwtOptions.Value;
        _redisService = redisService;
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

        var loginResponse = result.Value!;

        Response.Cookies.Append(
            "supportpilot.access_token",
            loginResponse.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(
                    _jwtSettings.AccessTokenExpirationMinutes),
                IsEssential = true
            });

        Response.Cookies.Append(
            "supportpilot.refresh_token",
            loginResponse.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(
                    _jwtSettings.RefreshTokenExpirationDays),
                IsEssential = true
            });

        return Ok(new
        {
            id = loginResponse.Id,
            firstName = loginResponse.FirstName,
            lastName = loginResponse.LastName,
            email = loginResponse.Email,
            role = loginResponse.Role,
            message = "Login successful."
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        CancellationToken cancellationToken)
    {
        var refreshTokenValue = Request.Cookies[
            "supportpilot.refresh_token"];

        var result = await _refreshTokenService.ExecuteAsync(
            new RefreshTokenRequest(refreshTokenValue ?? string.Empty),
            cancellationToken);

        if (result.IsFailure)
        {
            ClearAuthCookies();

            return Unauthorized(new
            {
                error = result.Error
            });
        }

        var tokens = result.Value!;

        Response.Cookies.Append(
            "supportpilot.access_token",
            tokens.AccessToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(
                    _jwtSettings.AccessTokenExpirationMinutes),
                IsEssential = true
            });

        Response.Cookies.Append(
            "supportpilot.refresh_token",
            tokens.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(
                    _jwtSettings.RefreshTokenExpirationDays),
                IsEssential = true
            });

        return Ok(new
        {
            message = "Tokens refreshed successfully."
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        CancellationToken cancellationToken)
    {
        var accessToken = Request.Cookies[
            "supportpilot.access_token"];

        var refreshToken = Request.Cookies[
            "supportpilot.refresh_token"];

        await BlacklistAccessTokenAsync(accessToken);

        await _logoutUserService.ExecuteAsync(
            refreshToken,
            cancellationToken);

        ClearAuthCookies();

        return Ok(new
        {
            message = "Logged out successfully."
        });
    }

    private async Task BlacklistAccessTokenAsync(string? accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return;

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(accessToken))
            return;

        var jwtToken = handler.ReadJwtToken(accessToken);

        var jti = jwtToken.Id;
        var remainingLifetime = jwtToken.ValidTo - DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(jti) ||
            remainingLifetime <= TimeSpan.Zero)
        {
            return;
        }

        var blacklistKey = $"blacklist:{jti}";

        await _redisService.SetAsync(
            blacklistKey,
            "revoked",
            remainingLifetime);
    }

    private void ClearAuthCookies()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        };

        Response.Cookies.Delete(
            "supportpilot.access_token",
            cookieOptions);

        Response.Cookies.Delete(
            "supportpilot.refresh_token",
            cookieOptions);
    }
}
