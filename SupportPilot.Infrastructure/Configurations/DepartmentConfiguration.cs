
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportPilot.Core.Entities;

namespace SupportPilot.Infrastructure.Configurations;

public sealed class DepartmentConfiguration
    : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(department => department.Id);

        builder.Property(department => department.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(department => department.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(department => department.Name)
            .IsUnique();

        builder.Property(department => department.CreatedAtUtc)
            .IsRequired();
    }
}
