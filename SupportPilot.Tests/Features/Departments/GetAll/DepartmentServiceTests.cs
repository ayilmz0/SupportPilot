
using System.Text.Json;
using NSubstitute;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Features.Departments.Common;
using SupportPilot.Application.Features.Departments.GetAll;
using SupportPilot.Core.Entities;

namespace SupportPilot.Tests.Features.Departments.GetAll;

public class DepartmentServiceTests
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRedisService _redisService;
    private readonly DepartmentService _service;

    public DepartmentServiceTests()
    {
        _departmentRepository =
            Substitute.For<IDepartmentRepository>();

        _redisService =
            Substitute.For<IRedisService>();

        _service = new DepartmentService(
            _departmentRepository,
            _redisService);
    }

    [Fact]
    public async Task ExecuteAsync_CacheHit_ShouldReturnCachedDepartments()
    {
        // Arrange
        var expectedDepartments = new List<DepartmentDto>
        {
            new(
                Guid.NewGuid(),
                "Technical Support",
                "Technical support requests",
                DateTime.UtcNow)
        };

        var cachedData =
            JsonSerializer.Serialize(expectedDepartments);

        _redisService
            .GetAsync("departments:active")
            .Returns(Task.FromResult<string?>(cachedData));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(
            "Technical Support",
            result[0].Name);

        await _departmentRepository
            .DidNotReceive()
            .GetAllActiveAsync(Arg.Any<CancellationToken>());

        await _redisService
            .DidNotReceive()
            .SetAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task ExecuteAsync_CacheMiss_ShouldFetchFromDatabaseAndCache()
    {
        // Arrange
        var departments = new List<Department>
        {
            new(
                "Technical Support",
                "Technical support requests")
        };

        _redisService
            .GetAsync("departments:active")
            .Returns(Task.FromResult<string?>(null));

        _departmentRepository
            .GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<Department>>(
                    departments));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(
            "Technical Support",
            result[0].Name);

        await _departmentRepository
            .Received(1)
            .GetAllActiveAsync(Arg.Any<CancellationToken>());

        await _redisService
            .Received(1)
            .SetAsync(
                "departments:active",
                Arg.Any<string>(),
                TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task ExecuteAsync_EmptyDatabase_ShouldReturnEmptyList()
    {
        // Arrange
        _redisService
            .GetAsync("departments:active")
            .Returns(Task.FromResult<string?>(null));

        _departmentRepository
            .GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<Department>>(
                    new List<Department>()));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Empty(result);

        await _departmentRepository
            .Received(1)
            .GetAllActiveAsync(Arg.Any<CancellationToken>());

        await _redisService
            .Received(1)
            .SetAsync(
                "departments:active",
                Arg.Any<string>(),
                TimeSpan.FromMinutes(10));
    }


    [Fact]
    public async Task ExecuteAsync_RedisReadFails_ShouldFetchFromDatabase()
    {
        // Arrange
        var departments = new List<Department>
    {
        new("Technical Support", "Technical support requests")
    };

        _redisService
            .GetAsync("departments:active")
            .Returns<Task<string?>>(
                _ => throw new InvalidOperationException("Redis unavailable"));

        _departmentRepository
            .GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<Department>>(departments));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Technical Support", result[0].Name);

        await _departmentRepository
            .Received(1)
            .GetAllActiveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidCachedJson_ShouldFetchFromDatabase()
    {
        // Arrange
        var departments = new List<Department>
    {
        new("Billing Support", "Billing related requests")
    };

        _redisService
            .GetAsync("departments:active")
            .Returns(Task.FromResult<string?>("{ invalid json"));

        _departmentRepository
            .GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<Department>>(departments));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Billing Support", result[0].Name);

        await _departmentRepository
            .Received(1)
            .GetAllActiveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RedisWriteFails_ShouldStillReturnDatabaseResult()
    {
        // Arrange
        var departments = new List<Department>
    {
        new("Account Support", "Account related requests")
    };

        _redisService
            .GetAsync("departments:active")
            .Returns(Task.FromResult<string?>(null));

        _departmentRepository
            .GetAllActiveAsync(Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyList<Department>>(departments));

        _redisService
            .SetAsync(
                "departments:active",
                Arg.Any<string>(),
                Arg.Any<TimeSpan>())
            .Returns<Task>(
                _ => throw new InvalidOperationException("Redis unavailable"));

        // Act
        var result = await _service.ExecuteAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Account Support", result[0].Name);
    }


    [Fact]
    public async Task ExecuteAsync_CancellationBeforeExecution_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        // Act
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ExecuteAsync(
                cancellationTokenSource.Token));

        // Assert
        Assert.NotNull(exception);

        await _redisService
            .DidNotReceive()
            .GetAsync(
                Arg.Any<string>(),
                Arg.Any<CancellationToken>());

        await _departmentRepository
            .DidNotReceive()
            .GetAllActiveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RedisReadCancelled_ShouldPropagateCancellation()
    {
        // Arrange
        using var cancellationTokenSource =
            new CancellationTokenSource();

        _redisService
            .GetAsync(
                "departments:active",
                Arg.Any<CancellationToken>())
            .Returns<Task<string?>>(
                _ => throw new OperationCanceledException());

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _service.ExecuteAsync(
                cancellationTokenSource.Token));

        // Assert
        await _departmentRepository
            .DidNotReceive()
            .GetAllActiveAsync(Arg.Any<CancellationToken>());

        await _redisService
            .DidNotReceive()
            .SetAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>());
    }


}
