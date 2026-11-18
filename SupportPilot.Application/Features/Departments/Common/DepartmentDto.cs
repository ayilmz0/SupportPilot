
namespace SupportPilot.Application.Features.Departments.Common;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAtUtc);
