using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Application.Common.Pagination;
using SupportPilot.Application.Common.Results;
using SupportPilot.Core.Enums;

namespace SupportPilot.Application.Features.Tickets.GetAllTickets;

public sealed class GetAllTicketsService
{
    private readonly ITicketRepository _ticketRepository;

    public GetAllTicketsService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }


    public async Task<Result<PagedResponse<GetAllTicketsResponse>>> ExecuteAsync(
        int page,
        int pageSize,
        TicketStatus? status,
        TicketPriority? priority,
        TicketCategory? category,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Sayfa numarası 1'den küçük olamaz.");

        if (pageSize < 1 || pageSize > 100)
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Sayfa boyutu 1 ile 100 arasında olmalıdır.");

        if (status.HasValue &&
            !Enum.IsDefined(typeof(TicketStatus), status.Value))
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Geçersiz ticket durumu.");

        if (priority.HasValue &&
            !Enum.IsDefined(typeof(TicketPriority), priority.Value))
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Geçersiz ticket önceliği.");

        if (category.HasValue &&
            !Enum.IsDefined(typeof(TicketCategory), category.Value))
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Geçersiz ticket kategorisi.");

        if (searchTerm?.Length > 200)
            return Result<PagedResponse<GetAllTicketsResponse>>
                .Failure("Arama metni 200 karakterden uzun olamaz.");

        searchTerm = searchTerm?.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
            searchTerm = null;

        var tickets = await _ticketRepository.GetAllAsync(
            page,
            pageSize,
            status,
            priority,
            category,
            searchTerm,
            cancellationToken);

        var totalCount = await _ticketRepository.CountAsync(
            status,
            priority,
            category,
            searchTerm,
            cancellationToken);

        var items = tickets
            .Select(ticket => new GetAllTicketsResponse(
                ticket.Id,
                ticket.Title,
                ticket.Status,
                ticket.Priority,
                ticket.Category,
                ticket.CustomerId,
                ticket.AssignedAgentId,
                ticket.DepartmentId,
                ticket.CreatedAtUtc))
            .ToList();

        var response = new PagedResponse<GetAllTicketsResponse>(
            items,
            page,
            pageSize,
            totalCount);

        return Result<PagedResponse<GetAllTicketsResponse>>
            .Success(response);
    }
}