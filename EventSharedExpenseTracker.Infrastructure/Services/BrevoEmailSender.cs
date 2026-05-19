using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace EventSharedExpenseTracker.Infrastructure.Services;

public class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public BrevoEmailSender(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var apiKey = _config["Brevo:ApiKey"];
        var senderEmail = _config["Brevo:SenderEmail"];
        var senderName = _config["Brevo:SenderName"];

        var payload = new
        {
            sender = new { name = senderName, email = senderEmail },
            to = new[] { new { email } },
            subject,
            htmlContent = htmlMessage
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
        request.Headers.Add("api-key", apiKey);
        request.Content = JsonContent.Create(payload);

        var response = await _httpClient.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Brevo email failed: {response.StatusCode} - {body}");
    }
}
