using EventSharedExpenseTracker.Tests;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class AuthorisationTests
{
    [Fact]
    public async Task Get_CreateTrip_WhenUnauthenticated_RedirectsToLogin()
    {
        await using var factory = new CustomWebApplicationFactory(useAuthentication: false);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Trips/Create");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);

        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Get_CreateTrip_WhenAuthenticated_ReturnsSuccess()
    {
        await using var factory = new CustomWebApplicationFactory(useAuthentication: true);

        var client = factory.CreateClient();

        var response = await client.GetAsync("/Trips/Create");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
