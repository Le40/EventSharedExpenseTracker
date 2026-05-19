
/*using System.Net;
using System.Net.Http.Json;

namespace CompanyStructure.IntegrationTests.Helpers;

public class TestApiClient
{
    private readonly HttpClient _client;

    public TestApiClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<NodeResponse> CreateTripAsync(
        string? name = null,
        string? code = null)
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        var request = new CreateCompanyRequest
        {
            Name = name ?? $"Company {unique}",
            Code = code ?? $"C-{unique}"
        };

        var response = await _client.PostAsJsonAsync("/api/companies", request,
            cancellationToken: TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var company = await response.Content.ReadFromJsonAsync<NodeResponse>(
            cancellationToken: TestContext.Current.CancellationToken);

        company.Should().NotBeNull();

        return company!;
    }
}*/
