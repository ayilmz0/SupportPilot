
using Microsoft.EntityFrameworkCore;
using SupportPilot.Infrastructure.Data;
using SupportPilot.Core.Entities;

namespace SupportPilot.Infrastructure.Data.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        if (!await context.Departments.AnyAsync(cancellationToken))
        {
            var department = new Department(
                "Technical Support",
                "Technical support requests");

            await context.Departments.AddAsync(
                department, cancellationToken);
        }

        if (!await context.Users.AnyAsync(cancellationToken))
        {
            var customer = new User(
                "Test",
                "Customer",
                "customer@supportpilot.local");

            await context.Users.AddAsync(
                customer, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
