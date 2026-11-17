using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Common.Interfaces;

public interface IRefreshTokenRepository
{
    Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task RotateAsync(
        RefreshToken currentToken,
        RefreshToken newToken,
        CancellationToken cancellationToken = default);

    Task RevokeByTokenHashAsync(
    string tokenHash,
    CancellationToken cancellationToken = default);
}