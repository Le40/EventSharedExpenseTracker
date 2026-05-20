using FluentAssertions;
using Tests;
using Xunit;

namespace Tests.Integration;

public class SmokeTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        try
        {
            _client = factory.CreateClient();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }
    }

    [Fact]
    public async Task App_Starts_And_HomePage_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
