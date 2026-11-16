
namespace SupportPilot.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(
        Guid userId,
        string email,
        string role);
}