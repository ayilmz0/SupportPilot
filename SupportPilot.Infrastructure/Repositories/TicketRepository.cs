
using Microsoft.EntityFrameworkCore;
using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Core.Entities;
using SupportPilot.Infrastructure.Data;

namespace SupportPilot.Infrastructure.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> CustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return _context.Users.AnyAsync(
            user => user.Id == customerId && user.IsActive,
            cancellationToken);
    }
}
