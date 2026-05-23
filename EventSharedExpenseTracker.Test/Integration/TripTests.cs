using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Tests.Factories;
using EventSharedExpenseTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class TripTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly FormTestHelper _formHelper;
    private readonly ITestOutputHelper _output; 
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public TripTests(ITestOutputHelper output)
    {
        _output = output;
        _factory = new CustomWebApplicationFactory(true);

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        _formHelper = new FormTestHelper(_client, _cancellationToken);
    }

    [Fact]
    public async Task Trip_Index_Works()
    {
        var response = await _client.GetAsync("/Trips", _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Create_Get_Works()
    {
        var response = await _client.GetAsync("/Trips/Create", _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Create_WhenValidationFails_ReturnsForm()
    {
        var form = await _formHelper.GetFormFieldsAsync("/Trips/Create");

        form["Name"] = "";
        form["DateFrom"] = DateTime.Today.ToString("yyyy-MM-dd");
        form["DateTo"] = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");

        var response = await _formHelper.PostFormAsync("/Trips/Create", form);

        var html = await response.Content.ReadAsStringAsync(_cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Name is required");
    }

    [Fact]
    public async Task Trip_Create_Post_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var form = await _formHelper.GetFormFieldsAsync("/Trips/Create");

        form["Name"] = "Summer Trip";
        form["DateFrom"] = DateTime.Today.ToString("yyyy-MM-dd");
        form["DateTo"] = DateTime.Today.AddDays(3).ToString("yyyy-MM-dd");

        var response = await _formHelper.PostFormAsync("/Trips/Create", form);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        // after POST
        //using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Trips.Any(t => t.Name == "Summer Trip").Should().BeTrue();
    }

    [Fact]
    public async Task Trip_Details_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var response = await _client.GetAsync($"/Trips/Details/{seed.TripId}", _cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Edit_Get_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var response = await _client.GetAsync($"/Trips/Edit/{seed.TripId}", TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Trip_Update_WhenValidationFails_ReturnsForm()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var form = await _formHelper.GetFormFieldsAsync($"/Trips/Edit/{seed.TripId}");

        form["Name"] = "";

        var response = await _formHelper.PostFormAsync($"/Trips/Edit/{seed.TripId}", form);

        var html = await response.Content.ReadAsStringAsync(_cancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("Name is required");
    }

    [Fact]
    public async Task Trip_Update_Post_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var form = await _formHelper.GetFormFieldsAsync($"/Trips/Edit/{seed.TripId}");

        form["Name"] = "Updated Trip";
        form["DateFrom"] = DateTime.Today.ToString("yyyy-MM-dd");
        form["DateTo"] = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");

        var response = await _formHelper.PostFormAsync($"/Trips/Edit/{seed.TripId}", form);

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Trips.Any(t => t.Name == "Updated Trip").Should().BeTrue();
    }

    [Fact]
    public async Task Trip_Delete_Works()
    {
        using var scope = _factory.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<TestDataSeeder>();

        var seed = await seeder.SeedTripWithParticipantsAsync(_cancellationToken);

        var response = await _formHelper.PostFormAsync($"/Trips/Delete/{seed.TripId}", new()
        {
            ["id"] = seed.TripId.ToString()
        });

        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Redirect,
            HttpStatusCode.OK);

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        db.Trips.Any(t => t.Id == seed.TripId)
            .Should().BeFalse();
    }

    private async Task PrintResponseAsync(string label, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"--- {label} ---");
        _output.WriteLine($"STATUS: {response.StatusCode}");
        _output.WriteLine($"LOCATION: {response.Headers.Location}");
        _output.WriteLine("BODY:");
        _output.WriteLine(body);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}