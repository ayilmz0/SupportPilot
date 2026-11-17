using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Common.Results;

namespace SupportPilot.Application.Features.Authentication.Logout;

public sealed class LogoutUserService
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public LogoutUserService(
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<Result> ExecuteAsync(
        string? refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return Result.Success();

        var tokenHash = _refreshTokenGenerator.Hash(refreshToken);

        await _refreshTokenRepository.RevokeByTokenHashAsync(
            tokenHash,
            cancellationToken);

        return Result.Success();
    }
}