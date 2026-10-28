using SupportPilot.Core.Enums;

namespace SupportPilot.Core.Entities;

public class Ticket
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public TicketStatus Status { get; private set; } = TicketStatus.Pending;

    public TicketPriority Priority { get; private set; } = TicketPriority.Medium;

    public TicketCategory Category { get; private set; } = TicketCategory.General;

    public Guid CustomerId { get; private set; }

    public Guid? AssignedAgentId { get; private set; }

    public Guid? DepartmentId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    private Ticket()
    {
        // EF Core için.
    }

    public Ticket(
        Guid customerId,
        string title,
        string description)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.", nameof(title));

        if (title.Length > 200)
            throw new ArgumentException("Title cannot exceed 200 characters.", nameof(title));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));

        CustomerId = customerId;
        Title = title.Trim();
        Description = description.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void AssignTo(Guid agentId, Guid departmentId)
    {
        if (agentId == Guid.Empty)
            throw new ArgumentException("Agent ID cannot be empty.", nameof(agentId));

        if (departmentId == Guid.Empty)
            throw new ArgumentException("Department ID cannot be empty.", nameof(departmentId));

        AssignedAgentId = agentId;
        DepartmentId = departmentId;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyAnalysis(
        TicketCategory category,
        TicketPriority priority)
    {
        Category = category;
        Priority = priority;

        if (Status == TicketStatus.Pending ||
            Status == TicketStatus.Analyzing)
        {
            Status = TicketStatus.Open;
        }

        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void StartProgress()
    {
        if (Status != TicketStatus.Open)
            throw new InvalidOperationException(
                "Only open tickets can be moved to in-progress.");

        Status = TicketStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Resolve()
    {
        if (Status != TicketStatus.InProgress)
            throw new InvalidOperationException(
                "Only in-progress tickets can be resolved.");

        Status = TicketStatus.Resolved;
        ResolvedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}