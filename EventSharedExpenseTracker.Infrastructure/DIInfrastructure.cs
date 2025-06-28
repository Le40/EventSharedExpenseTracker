using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Infrastructure.Data.Repositories;
using EventSharedExpenseTracker.Infrastructure.Identity;
using EventSharedExpenseTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Resend;

namespace EventSharedExpenseTracker.Infrastructure;

public static class DIInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //// DB
        var connectionString = configuration.GetConnectionString("LocalConnection") ?? throw new InvalidOperationException("Connection string 'LocalConnection' not found.");
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            connectionString = configuration.GetConnectionString("AzureConnection") ?? throw new InvalidOperationException("Connection string 'AzureConnection' not found.");
        //var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString));

        // IDENTITY
        services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<UserManager<ApplicationUser>, CustomUserManager<ApplicationUser>>();
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsPrincipalFactory>();

        // IMAGE UPLOAD
        services.AddScoped<IImageService, ImageService>();

        // EMAIL SENDER
        services.AddTransient<IEmailSender, EmailSender>();
        //services.Configure<ResendClientOptions>(options =>
        //{
        //   options.ApiToken = configuration["ResendKey"];
        //});

        services.AddHttpClient<ResendClient>(); // inject HttpClient
        services.AddTransient<IEmailSender, EmailSender>();
        services.Configure<AuthMessageSenderOptions>(configuration);

        // REPOSITORIES
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITripRepository, TripRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
