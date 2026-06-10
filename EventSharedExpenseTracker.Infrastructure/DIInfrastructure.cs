using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Infrastructure.Data.Repositories;
using EventSharedExpenseTracker.Infrastructure.Identity;
using EventSharedExpenseTracker.Infrastructure.Seed;
using EventSharedExpenseTracker.Infrastructure.Services;
using EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService;
using EventSharedExpenseTracker.Infrastructure.Services.ExchangeRateService.Providers.ExchangeRateApi;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventSharedExpenseTracker.Infrastructure;

public static class DIInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        //// DB
        if (!environment.IsEnvironment("Testing"))
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        // IDENTITY
        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<UserManager<ApplicationUser>, CustomUserManager<ApplicationUser>>();
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

        // IMAGE UPLOAD
        services.AddScoped<IImageService, ImageService>();

        // EMAIL SENDER
        //services.AddHttpClient<IEmailSender, BrevoEmailSender>();

        //services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddScoped<IExchangeRateService, ExchangeRateService>();
        services.AddHttpClient<IExchangeRateApiProvider, ExchangeRateApiProvider>();

        // REPOSITORIES
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IExpenseAiService, OpenAiExpenseAiService>();

        services.AddScoped<DemoDataSeeder>();

        return services;
    }
}
