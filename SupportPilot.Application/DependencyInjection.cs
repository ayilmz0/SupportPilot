using Microsoft.Extensions.DependencyInjection;
using SupportPilot.Application.Features.Authentication.Login;
using SupportPilot.Application.Features.Authentication.Register;
using SupportPilot.Application.Features.Tickets.CreateTicket;
using SupportPilot.Application.Features.Tickets.GetAllTickets;
using SupportPilot.Application.Features.Tickets.GetTicketById;

namespace SupportPilot.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<CreateTicketService>();
        services.AddScoped<GetTicketByIdService>();
        services.AddScoped<GetAllTicketsService>();
        services.AddScoped<RegisterUserService>();
        services.AddScoped<LoginUserService>();

        return services;
    }
}