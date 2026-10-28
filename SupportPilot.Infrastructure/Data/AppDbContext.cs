
using Microsoft.EntityFrameworkCore;
using SupportPilot.Core.Entities;

namespace SupportPilot.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Department> Departments => Set<Department>();
}
