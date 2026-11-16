using Microsoft.EntityFrameworkCore;
using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Core.Entities;
using SupportPilot.Infrastructure.Data;
using SupportPilot.Core.Enums;

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

    public Task<Ticket?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(
                ticket => ticket.Id == id,
                cancellationToken);
    }

    private IQueryable<Ticket> ApplyFilters(
        IQueryable<Ticket> query,
        TicketStatus? status,
        TicketPriority? priority,
        TicketCategory? category,
        string? searchTerm)
    {
        if (status.HasValue)
            query = query.Where(ticket => ticket.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(ticket => ticket.Priority == priority.Value);

        if (category.HasValue)
            query = query.Where(ticket => ticket.Category == category.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(ticket =>
                ticket.Title.Contains(searchTerm));
        }

        return query;
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        int page,
        int pageSize,
        TicketStatus? status,
        TicketPriority? priority,
        TicketCategory? category,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        return await ApplyFilters(
                _context.Tickets,
                status,
                priority,
                category,
                searchTerm)
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountAsync(
        TicketStatus? status,
        TicketPriority? priority,
        TicketCategory? category,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        return ApplyFilters(
                _context.Tickets,
                status,
                priority,
                category,
                searchTerm)
            .CountAsync(cancellationToken);
    }
}
