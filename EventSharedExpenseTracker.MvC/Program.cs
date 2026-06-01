using Azure.Identity;
using EventSharedExpenseTracker.Application;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Extensions;
using EventSharedExpenseTracker.Infrastructure;
using EventSharedExpenseTracker.MvC.Factories;
using EventSharedExpenseTracker.MvC.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
// LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// AZURE KEY VAULT
/*if (builder.Environment.IsProduction())
{*/
    var vaultUri = Environment.GetEnvironmentVariable("VaultUri");

    if (!string.IsNullOrWhiteSpace(vaultUri))
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(vaultUri),
            new DefaultAzureCredential());
    }
//}

builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddScoped<IRequestContext, RequestContextService>();
builder.Services.AddScoped<ExpenseFormFactory>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseAppExceptionHandling();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// LOGGING REQUESTS
app.UseAppRequestLogging();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Trip}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();

public partial class Program { }

