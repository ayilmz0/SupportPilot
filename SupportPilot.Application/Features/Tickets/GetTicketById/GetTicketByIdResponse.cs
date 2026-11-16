
using SupportPilot.Core.Enums;

namespace SupportPilot.Application.Features.Tickets.GetTicketById;

public sealed record GetTicketByIdResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategory Category,
    Guid CustomerId,
    Guid? AssignedAgentId,
    Guid? DepartmentId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? ResolvedAtUtc);