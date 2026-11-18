using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportPilot.Application.Abstractions.Persistence;
using SupportPilot.Application.Common.Interfaces;
using SupportPilot.Infrastructure.Data;
using SupportPilot.Infrastructure.Data.Repositories;
using SupportPilot.Infrastructure.Repositories;
using SupportPilot.Infrastructure.Security;
using SupportPilot.Infrastructure.Redis;

namespace SupportPilot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration
            .GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection connection string bulunamadı.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<ITicketRepository, TicketRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRedisService, RedisService>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();

        services.Configure<JwtSettings>(
            configuration.GetSection(JwtSettings.SectionName));

        var redisConnectionString =
    configuration["Redis:ConnectionString"]
    ?? throw new InvalidOperationException(
        "Redis connection string is missing.");

        services.AddSingleton(
            new RedisConnection(redisConnectionString));

        return services;
    }
}