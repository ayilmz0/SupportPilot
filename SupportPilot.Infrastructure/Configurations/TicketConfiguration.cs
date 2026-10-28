
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPilot.Core.Entities;

namespace SupportPilot.Infrastructure.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets");

        builder.HasKey(ticket => ticket.Id);

        builder.Property(ticket => ticket.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ticket => ticket.Description)
            .IsRequired();

        builder.Property(ticket => ticket.Status)
            .IsRequired();

        builder.Property(ticket => ticket.Priority)
            .IsRequired();

        builder.Property(ticket => ticket.Category)
            .IsRequired();

        builder.Property(ticket => ticket.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(ticket => ticket.CustomerId);

        builder.HasIndex(ticket => new
        {
            ticket.Status,
            ticket.Priority
        });
    }
}
