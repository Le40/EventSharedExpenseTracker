using EventSharedExpenseTracker.Tests;
using EventSharedExpenseTracker.Tests.Factories;
using EventSharedExpenseTracker.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace EventSharedExpenseTracker.Tests.Integration;

public class AuthTests 
{
    [Fact]
    public async Task HomePage_ReturnsSuccess()
    {
        var client = new CustomWebApplicationFactory(false).CreateClient();
        var response = await client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Get_CreateTrip_WhenUnauthenticated_RedirectsToLogin()
    {
        await using var factory = new CustomWebApplicationFactory(false);

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
}
