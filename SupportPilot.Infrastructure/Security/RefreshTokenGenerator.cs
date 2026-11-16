
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using SupportPilot.Application.Common.Interfaces;

namespace SupportPilot.Infrastructure.Security;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly JwtSettings _settings;

    public RefreshTokenGenerator(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
    }

    public string Generate()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }

    public DateTime GetExpirationDateUtc()
    {
        return DateTime.UtcNow.AddDays(
            _settings.RefreshTokenExpirationDays);
    }
}