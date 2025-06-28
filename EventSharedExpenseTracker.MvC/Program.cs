using EventSharedExpenseTracker.Application;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure;
using EventSharedExpenseTracker.MvC.ActionFilters;
using EventSharedExpenseTracker.MvC.Services;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

// AZURE KEY VAULT
//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri"));
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());


builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddControllersWithViews();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CustomValidationActionFilter>();
builder.Services.AddScoped<IRequestContext, RequestContextService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Trip}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
