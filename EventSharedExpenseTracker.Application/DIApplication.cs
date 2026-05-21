using Microsoft.Extensions.DependencyInjection;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Application.Trips;
using EventSharedExpenseTracker.Application.Friends;

namespace EventSharedExpenseTracker.Application;

public static class DIApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //services.AddScoped<IAuthorisationService, AuthorisationRules>();
        //services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<ITripService, TripService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<IFriendService, FriendService>();
        return services;

    }
}
