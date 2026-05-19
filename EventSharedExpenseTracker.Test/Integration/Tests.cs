using EventSharedExpenseTracker.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class Tests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorisationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Get_CreateTrip_WhenUnauthenticated_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/Trips/Create");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Get_CreateTrip_WhenAuthenticated_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Trips/Create");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}