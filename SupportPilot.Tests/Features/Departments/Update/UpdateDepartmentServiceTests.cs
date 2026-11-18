using NSubstitute;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Features.Departments.Update;
using SupportPilot.Core.Entities;

namespace SupportPilot.Tests.Features.Departments.Update;

public class UpdateDepartmentServiceTests
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRedisService _redisService;
    private readonly UpdateDepartmentService _service;

    public UpdateDepartmentServiceTests()
    {
        _departmentRepository =
            Substitute.For<IDepartmentRepository>();

        _redisService =
            Substitute.For<IRedisService>();

        _service = new UpdateDepartmentService(
            _departmentRepository,
            _redisService);
    }

    [Fact]
    public async Task ExecuteAsync_ValidDepartment_ShouldUpdateAndInvalidateCache()
    {
        // Arrange
        var departmentId = Guid.NewGuid();

        var department = new Department(
            "Technical Support",
            "Old description");

        _departmentRepository
            .GetByIdAsync(
                departmentId,
                Arg.Any<CancellationToken>())
            .Returns(department);

        // Act
        var result = await _service.ExecuteAsync(
            departmentId,
            "Customer Support",
            "New description");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Customer Support", department.Name);
        Assert.Equal("New description", department.Description);

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
        var result = await _service.ExecuteAsync(
            departmentId,
            "Customer Support",
            "New description");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Department not found.", result.Error);

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
