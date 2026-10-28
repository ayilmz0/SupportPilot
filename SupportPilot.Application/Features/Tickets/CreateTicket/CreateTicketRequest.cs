namespace SupportPilot.Application.Features.Tickets.CreateTicket;

public sealed record CreateTicketRequest(
    Guid CustomerId,
    string Title,
    string Description);