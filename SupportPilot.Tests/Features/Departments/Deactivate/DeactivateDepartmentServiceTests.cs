using NSubstitute;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Features.Departments.Deactivate;
using SupportPilot.Core.Entities;

namespace SupportPilot.Tests.Features.Departments.Deactivate;

public class DeactivateDepartmentServiceTests
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRedisService _redisService;
    private readonly DeactivateDepartmentService _service;

    public DeactivateDepartmentServiceTests()
    {
        _departmentRepository =
            Substitute.For<IDepartmentRepository>();

        _redisService =
            Substitute.For<IRedisService>();

        _service = new DeactivateDepartmentService(
            _departmentRepository,
            _redisService);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveDepartment_ShouldDeactivateAndInvalidateCache()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department(
            "Technical Support",
            "Technical support requests");

        _departmentRepository
            .GetByIdAsync(
                departmentId,
                Arg.Any<CancellationToken>())
            .Returns(department);

        // Act
        var result = await _service.ExecuteAsync(departmentId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(department.IsActive);

        await _departmentRepository
            .Received(1)
            .UpdateAsync(
                department,
                Arg.Any<CancellationToken>());

        await _departmentRepository
            .Received(1)
            .SaveChangesAsync(Arg.Any<CancellationToken>());

        await _redisService
            .Received(1)
            .RemoveAsync("departments:active");
    }

    [Fact]
    public async Task ExecuteAsync_DepartmentNotFound_ShouldReturnFailure()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        _departmentRepository
            .GetByIdAsync(
                departmentId,
                Arg.Any<CancellationToken>())
            .Returns((Department?)null);

        // Act
        var result = await _service.ExecuteAsync(departmentId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Department not found.", result.Error);

        await _departmentRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<Department>(),
                Arg.Any<CancellationToken>());

        await _redisService
            .DidNotReceive()
            .RemoveAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task ExecuteAsync_AlreadyInactiveDepartment_ShouldReturnFailure()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department(
            "Technical Support",
            "Technical support requests");

        department.Deactivate();

        _departmentRepository
            .GetByIdAsync(
                departmentId,
                Arg.Any<CancellationToken>())
            .Returns(department);

        // Act
        var result = await _service.ExecuteAsync(departmentId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            "Department is already inactive.",
            result.Error);

        await _departmentRepository
            .DidNotReceive()
            .UpdateAsync(
                Arg.Any<Department>(),
                Arg.Any<CancellationToken>());

        await _departmentRepository
            .DidNotReceive()
            .SaveChangesAsync(Arg.Any<CancellationToken>());

        await _redisService
            .DidNotReceive()
            .RemoveAsync(Arg.Any<string>());
    }
}
