
using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Application.Common.Results;

namespace SupportPilot.Application.Features.Tickets.GetTicketById;

public sealed class GetTicketByIdService
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<GetTicketByIdResponse>> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return Result<GetTicketByIdResponse>.Failure(
                "Geçersiz ticket kimliği.");

        var ticket = await _ticketRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (ticket is null)
            return Result<GetTicketByIdResponse>.Failure(
                "Ticket bulunamadı.");

        var response = new GetTicketByIdResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.Category,
            ticket.CustomerId,
            ticket.AssignedAgentId,
            ticket.DepartmentId,
            ticket.CreatedAtUtc,
            ticket.UpdatedAtUtc,
            ticket.ResolvedAtUtc);

        return Result<GetTicketByIdResponse>.Success(response);
    }
}