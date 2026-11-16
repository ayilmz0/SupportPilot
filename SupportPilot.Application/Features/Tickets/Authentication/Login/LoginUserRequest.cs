
namespace SupportPilot.Application.Features.Authentication.Login;

public sealed record LoginUserRequest(
    string Email,
    string Password);