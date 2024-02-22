using Microsoft.Extensions.DependencyInjection;

using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Validation;
using EventSharedExpenseTracker.Application.Services;
using EventSharedExpenseTracker.Application.Services.Interfaces;

namespace EventSharedExpenseTracker.Application;

public static class DIApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthorisationServ, AuthorisationServ>();
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        return services;

    }
}
