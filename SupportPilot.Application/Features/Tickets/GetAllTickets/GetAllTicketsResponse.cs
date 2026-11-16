
using SupportPilot.Core.Enums;

namespace SupportPilot.Application.Features.Tickets.GetAllTickets;

public sealed record GetAllTicketsResponse(
    Guid Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    TicketCategory Category,
    Guid CustomerId,
    Guid? AssignedAgentId,
    Guid? DepartmentId,
    DateTime CreatedAtUtc);