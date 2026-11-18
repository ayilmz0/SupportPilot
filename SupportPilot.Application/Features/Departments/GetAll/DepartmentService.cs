using System.Text.Json;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Application.Features.Departments.Common;

namespace SupportPilot.Application.Features.Departments.GetAll;

public sealed class DepartmentService
{
    private const string CacheKey = "departments:active";

    private static readonly TimeSpan CacheExpiration =
        TimeSpan.FromMinutes(10);

    private readonly IDepartmentRepository _departmentRepository;
    private readonly IRedisService _redisService;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IRedisService redisService)
    {
        _departmentRepository = departmentRepository;
        _redisService = redisService;
    }

    public async Task<IReadOnlyList<DepartmentDto>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? cachedData = null;

        // 1. Redis cache okumayı dene.
        try
        {
            cachedData = await _redisService.GetAsync(
                CacheKey,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Redis hatası: SQL üzerinden devam et.
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 2. Cache verisini deserialize et.
        if (!string.IsNullOrWhiteSpace(cachedData))
        {
            try
            {
                var cachedDepartments =
                    JsonSerializer.Deserialize<List<DepartmentDto>>(
                        cachedData);

                if (cachedDepartments is not null)
                    return cachedDepartments;
            }
            catch (JsonException)
            {
                // Bozuk cache verisi: SQL üzerinden devam et.
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        // 3. SQL Server'dan verileri getir.
        var departments =
            await _departmentRepository.GetAllActiveAsync(
                cancellationToken);

        var departmentDtos = departments
            .Select(department => new DepartmentDto(
                department.Id,
                department.Name,
                department.Description,
                department.CreatedAtUtc))
            .ToList();

        cancellationToken.ThrowIfCancellationRequested();

        // 4. Redis'e yazmayı dene.
        try
        {
            var serializedData =
                JsonSerializer.Serialize(departmentDtos);

            await _redisService.SetAsync(
                CacheKey,
                serializedData,
                CacheExpiration,
                cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Cache yazma hatası sonucu engellemez.
        }

        return departmentDtos;
    }
}
