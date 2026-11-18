using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Common.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);

    Task<Department?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Department department,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
