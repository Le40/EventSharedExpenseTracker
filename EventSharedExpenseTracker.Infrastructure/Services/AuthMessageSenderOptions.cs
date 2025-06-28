
namespace EventSharedExpenseTracker.Infrastructure.Services;

public class AuthMessageSenderOptions
{
    public string? SendGridKey { get; set; }
    public string? ResendKey { get; set; }
}
