using SupportPilot.Core.Enums;

namespace SupportPilot.Application.Features.Authentication.Login;

public sealed record LoginUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    UserRole Role,
    string AccessToken,
    string RefreshToken);