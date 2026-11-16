using SupportPilot.Core.Entities;
using SupportPilot.Core.Enums;

namespace SupportPilot.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);

    Task<bool> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(
     int page,
     int pageSize,
     TicketStatus? status,
     TicketPriority? priority,
     TicketCategory? category,
     string? searchTerm,
     CancellationToken cancellationToken = default);

    Task<int> CountAsync(
    TicketStatus? status,
    TicketPriority? priority,
    TicketCategory? category,
    string? searchTerm,
    CancellationToken cancellationToken = default);
}