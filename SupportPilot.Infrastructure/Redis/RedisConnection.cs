
using StackExchange.Redis;

namespace SupportPilot.Infrastructure.Redis;

public sealed class RedisConnection : IAsyncDisposable
{
    private readonly ConnectionMultiplexer _connection;

    public RedisConnection(string connectionString)
    {
        _connection = ConnectionMultiplexer.Connect(connectionString);
    }

    public IDatabase Database => _connection.GetDatabase();

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
