using System.Security.Claims;

namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IRequestContext
{
    string? UserName { get; }
    int UserId { get; }
    ClaimsPrincipal User { get; }
}
