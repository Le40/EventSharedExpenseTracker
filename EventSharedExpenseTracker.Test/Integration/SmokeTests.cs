using EventSharedExpenseTracker.Tests;
using FluentAssertions;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
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
}
