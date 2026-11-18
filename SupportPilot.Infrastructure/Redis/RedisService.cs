using SupportPilot.Application.Common.Interfaces;

namespace SupportPilot.Infrastructure.Redis;

public sealed class RedisService : IRedisService
{
    private readonly RedisConnection _redisConnection;

    public RedisService(RedisConnection redisConnection)
    {
        _redisConnection = redisConnection;
    }

    public async Task SetAsync(
        string key,
        string value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _redisConnection.Database.StringSetAsync(
            key,
            value,
            expiration)
            .WaitAsync(cancellationToken);
    }

    public async Task<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var value = await _redisConnection.Database
            .StringGetAsync(key)
            .WaitAsync(cancellationToken);

        return value.IsNull ? null : value.ToString();
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await _redisConnection.Database
            .KeyDeleteAsync(key)
            .WaitAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _redisConnection.Database
            .KeyExistsAsync(key)
            .WaitAsync(cancellationToken);
    }
}
