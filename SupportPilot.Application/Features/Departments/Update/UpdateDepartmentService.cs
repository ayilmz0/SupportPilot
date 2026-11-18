using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Common.Results;

namespace SupportPilot.Application.Features.Departments.Update;

public sealed class UpdateDepartmentService
{
    private const string CacheKey = "departments:active";

    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRedisService _redisService;

    public UpdateDepartmentService(
        IDepartmentRepository departmentRepository,
        IRedisService redisService)
    {
        _departmentRepository = departmentRepository;
        _redisService = redisService;
    }

    public async Task<Result> ExecuteAsync(
        Guid departmentId,
        string name,
        string description,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var department = await _departmentRepository.GetByIdAsync(
            departmentId,
            cancellationToken);

        if (department is null)
            return Result.Failure("Department not found.");

        department.Update(name, description);

        await _departmentRepository.UpdateAsync(
            department,
            cancellationToken);

        await _departmentRepository.SaveChangesAsync(
            cancellationToken);

        await _redisService.RemoveAsync(
            CacheKey,
            cancellationToken);

        return Result.Success();
    }
}
