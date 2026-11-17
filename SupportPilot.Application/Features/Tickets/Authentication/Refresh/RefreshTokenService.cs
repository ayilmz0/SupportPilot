using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Common.Results;
using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Features.Authentication.Refresh;

public sealed class RefreshTokenService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RefreshTokenService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IRefreshTokenGenerator refreshTokenGenerator,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _refreshTokenGenerator = refreshTokenGenerator;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

public async Task<Result<RefreshTokenResponse>> ExecuteAsync(
    RefreshTokenRequest request,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<RefreshTokenResponse>.Failure(
                "Refresh token is required.");

        var tokenHash = _refreshTokenGenerator.Hash(
            request.RefreshToken);

        var currentToken = await _refreshTokenRepository.GetByTokenHashAsync(
            tokenHash,
            cancellationToken);

        if (currentToken is null || !currentToken.IsActive)
            return Result<RefreshTokenResponse>.Failure(
                "Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(
            currentToken.UserId,
            cancellationToken);

        if (user is null || !user.IsActive)
            return Result<RefreshTokenResponse>.Failure(
                "Invalid or expired refresh token.");

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(
            user.Id,
            user.Email,
            user.Role.ToString());

        var newRefreshTokenValue = _refreshTokenGenerator.Generate();

        var newRefreshTokenHash = _refreshTokenGenerator.Hash(
            newRefreshTokenValue);

        var newRefreshToken = new RefreshToken(
            user.Id,
            newRefreshTokenHash,
            _refreshTokenGenerator.GetExpirationDateUtc());

        await _refreshTokenRepository.RotateAsync(
            currentToken,
            newRefreshToken,
            cancellationToken);

        return Result<RefreshTokenResponse>.Success(
            new RefreshTokenResponse(
                accessToken,
                newRefreshTokenValue));
    }
}