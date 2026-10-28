using NSubstitute;
using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Application.Features.Tickets.CreateTicket;
using SupportPilot.Core.Entities;

namespace SupportPilot.Tests.Features.Tickets.CreateTicket;

public class CreateTicketServiceTests
{
    private readonly ITicketRepository _ticketRepository;
    private readonly CreateTicketService _service;

    public CreateTicketServiceTests()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _service = new CreateTicketService(_ticketRepository);
    }

    [Fact]
    public async Task ExecuteAsync_ValidRequest_ShouldCreateTicket()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var request = new CreateTicketRequest(
            customerId,
            "Login problem",
            "I cannot log in to my account.");

        _ticketRepository
            .CustomerExistsAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        _ticketRepository
            .AddAsync(Arg.Any<Ticket>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        await _ticketRepository
            .Received(1)
            .AddAsync(
                Arg.Is<Ticket>(ticket =>
                    ticket.Title == "Login problem" &&
                    ticket.CustomerId == customerId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyTitle_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateTicketRequest(
            Guid.NewGuid(),
            "   ",
            "I cannot log in.");

        // Act
        var result = await _service.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Ticket başlığı boş olamaz.", result.Error);

        await _ticketRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Ticket>(),
                Arg.Any<CancellationToken>());

        await _ticketRepository
            .DidNotReceive()
            .CustomerExistsAsync(
                Arg.Any<Guid>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NonExistingCustomer_ShouldReturnFailure()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        var request = new CreateTicketRequest(
            customerId,
            "Login problem",
            "I cannot log in.");

        _ticketRepository
            .CustomerExistsAsync(customerId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act
        var result = await _service.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Müşteri bulunamadı.", result.Error);

        await _ticketRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Ticket>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyCustomerId_ShouldReturnFailure()
    {
        // Arrange
        var request = new CreateTicketRequest(
            Guid.Empty,
            "Login problem",
            "I cannot log in.");

        // Act
        var result = await _service.ExecuteAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Geçersiz müşteri kimliği.", result.Error);

        await _ticketRepository
            .DidNotReceive()
            .AddAsync(
                Arg.Any<Ticket>(),
                Arg.Any<CancellationToken>());
    }
}
