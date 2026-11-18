using Microsoft.Extensions.DependencyInjection;
using SupportPilot.Application.Features.Authentication.Login;
using SupportPilot.Application.Features.Authentication.Logout;
using SupportPilot.Application.Features.Authentication.Refresh;
using SupportPilot.Application.Features.Authentication.Register;
using SupportPilot.Application.Features.Departments.Deactivate;
using SupportPilot.Application.Features.Departments.GetAll;
using SupportPilot.Application.Features.Departments.Update;
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
        services.AddScoped<RefreshTokenService>();
        services.AddScoped<LogoutUserService>();
        services.AddScoped<DepartmentService>();
        services.AddScoped<UpdateDepartmentService>();
        services.AddScoped<DeactivateDepartmentService>();

        return services;
    }
}