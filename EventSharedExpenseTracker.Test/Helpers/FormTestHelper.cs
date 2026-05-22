using AngleSharp.Html.Parser;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;

namespace EventSharedExpenseTracker.Tests.Helpers;

public class FormTestHelper
{
    private readonly HttpClient _client;
    private readonly CancellationToken _cancellationToken;

    public FormTestHelper(HttpClient client, CancellationToken cancellationToken)
    {
        _client = client;
        _cancellationToken = cancellationToken;
    }

    public async Task<Dictionary<string, string>> GetFormFieldsAsync(string url)
    {
        var response = await _client.GetAsync(url, _cancellationToken);

        //response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.StatusCode.Should().Be(
    HttpStatusCode.OK,
    $"GET {url} should return form, but got {response.StatusCode} and redirected to {response.Headers.Location}");

        var html = await response.Content.ReadAsStringAsync(_cancellationToken);

        var parser = new HtmlParser();
        var document = await parser.ParseDocumentAsync(html, _cancellationToken);

        var fields = new Dictionary<string, string>();

        foreach (var element in document.QuerySelectorAll("input, select, textarea"))
        {
            var name = element.GetAttribute("name");

            if (string.IsNullOrWhiteSpace(name))
                continue;

            var value = element.GetAttribute("value") ?? "";

            if (element.NodeName.Equals("select", StringComparison.OrdinalIgnoreCase))
            {
                var selected = element.QuerySelector("option[selected]");
                value = selected?.GetAttribute("value") ?? "";
            }

            fields[name] = value;
        }

        return fields;
    }

    public async Task<HttpResponseMessage> PostFormAsync(string url, Dictionary<string, string> formData)
    {
        using var content = new FormUrlEncodedContent(formData);

        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        return await _client.PostAsync(url, content);
    }
}