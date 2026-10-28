using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);

    Task<bool> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default);
}