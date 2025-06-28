using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Resend;

namespace EventSharedExpenseTracker.Infrastructure.Services;

/*public class EmailSender_Resend : IEmailSender
{
    private readonly ILogger _logger;
    private readonly ResendClient _resendClient;

    public EmailSender_Resend(IOptions<AuthMessageSenderOptions> optionsAccessor,
                       ILogger<EmailSender_Resend> logger, ResendClient resendClient)
    {
        Options = optionsAccessor.Value;
        _logger = logger;
        _resendClient = resendClient;
    }

    public AuthMessageSenderOptions Options { get; } //Set with Secret Manager.

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        if (string.IsNullOrEmpty(Options.ResendKey))
        {
            throw new Exception("Null ResendKey");
        }
        await Execute(Options.ResendKey, subject, message, toEmail);
    }

    public async Task Execute(string apiKey, string subject, string message, string toEmail)
    {
        var client = new ResendClient(new ResendClientOptions
        {
            ApiToken = apiKey
        });
        var msg = new EmailMessage()
        {
            From = "lefooo@gmail.com",
            To = { toEmail },
            Subject = subject,
            HtmlBody = message
        };
        //msg.AddTo(new EmailAddress(toEmail));

        // Disable click tracking.
        // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
        //msg.SetClickTracking(false, false);
        var response = await _resendClient.EmailSendAsync(msg);
        _logger.LogInformation(response.IsSuccessStatusCode
                               ? $"Email to {toEmail} queued successfully!"
                               : $"Failure Email to {toEmail}");
    }
}*/
