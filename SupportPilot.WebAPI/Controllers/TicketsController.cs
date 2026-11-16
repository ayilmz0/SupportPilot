using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.Features.Tickets.CreateTicket;
using SupportPilot.Application.Features.Tickets.GetTicketById;
using SupportPilot.Application.Features.Tickets.GetAllTickets;
using SupportPilot.Core.Enums;

namespace SupportPilot.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly CreateTicketService _createTicketService;
    private readonly GetTicketByIdService _getTicketByIdService;
    private readonly GetAllTicketsService _getAllTicketsService;

    public TicketsController(
        CreateTicketService createTicketService,
        GetTicketByIdService getTicketByIdService,
        GetAllTicketsService getAllTicketsService)
    {
        _createTicketService = createTicketService;
        _getTicketByIdService = getTicketByIdService;
        _getAllTicketsService = getAllTicketsService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTicketRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _createTicketService.ExecuteAsync(
            request,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value },
            new { id = result.Value });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _getTicketByIdService.ExecuteAsync(
            id,
            cancellationToken);

        if (result.IsFailure)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] TicketStatus? status = null,
    [FromQuery] TicketPriority? priority = null,
    [FromQuery] TicketCategory? category = null,
    [FromQuery] string? searchTerm = null,
    CancellationToken cancellationToken = default)
    {
        var result = await _getAllTicketsService.ExecuteAsync(
            page,
            pageSize,
            status,
            priority,
            category,
            searchTerm,
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}