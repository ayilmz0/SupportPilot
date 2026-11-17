
using Microsoft.EntityFrameworkCore;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Core.Entities;
using SupportPilot.Infrastructure.Data;

namespace SupportPilot.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(
            x => x.TokenHash == tokenHash,
            cancellationToken);
    }

    public async Task RotateAsync(
    RefreshToken currentToken,
    RefreshToken newToken,
    CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        currentToken.Revoke();

        await _context.RefreshTokens.AddAsync(
            newToken,
            cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RevokeByTokenHashAsync(
    string tokenHash,
    CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                x => x.TokenHash == tokenHash,
                cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive)
            return;

        refreshToken.Revoke();

        await _context.SaveChangesAsync(cancellationToken);
    }
}