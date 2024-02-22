using System.Security.Claims;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IRequestContext
{
    string? UserName { get; }
    int UserId { get; }
    ClaimsPrincipal User { get; }
}
