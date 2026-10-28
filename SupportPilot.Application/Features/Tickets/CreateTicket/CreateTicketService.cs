using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Application.Common.Results;
using SupportPilot.Core.Entities;

namespace SupportPilot.Application.Features.Tickets.CreateTicket;

public sealed class CreateTicketService
{
    private readonly ITicketRepository _ticketRepository;

    public CreateTicketService(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<Result<Guid>> ExecuteAsync(
        CreateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CustomerId == Guid.Empty)
            return Result<Guid>.Failure("Geçersiz müşteri kimliği.");

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<Guid>.Failure("Ticket başlığı boş olamaz.");

        if (request.Title.Trim().Length > 200)
            return Result<Guid>.Failure(
                "Ticket başlığı 200 karakterden uzun olamaz.");

        if (string.IsNullOrWhiteSpace(request.Description))
            return Result<Guid>.Failure("Ticket açıklaması boş olamaz.");

        var customerExists = await _ticketRepository.CustomerExistsAsync(
            request.CustomerId,
            cancellationToken);

        if (!customerExists)
            return Result<Guid>.Failure("Müşteri bulunamadı.");

        var ticket = new Ticket(
            request.CustomerId,
            request.Title,
            request.Description);

        await _ticketRepository.AddAsync(ticket, cancellationToken);

        return Result<Guid>.Success(ticket.Id);
    }
}