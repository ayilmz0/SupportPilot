namespace SupportPilot.Application.Features.Authentication.Refresh;

public sealed record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken);