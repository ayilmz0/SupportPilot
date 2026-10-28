namespace SupportPilot.Core.Entities;

public class Department
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAtUtc { get; private set; }

    private Department()
    {
        // EF Core için.
    }

    public Department(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name cannot be empty.");

        if (name.Length > 100)
            throw new ArgumentException(
                "Department name cannot exceed 100 characters.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}