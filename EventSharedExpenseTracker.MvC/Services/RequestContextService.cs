using EventSharedExpenseTracker.Application.Common.Interfaces;
using System.Security.Claims;

namespace EventSharedExpenseTracker.MvC.Services;

public class RequestContextService : IRequestContext
{
    private readonly HttpContext _httpContext;

    public RequestContextService(IHttpContextAccessor accessor)
    {
        _httpContext = accessor.HttpContext ?? throw new ArgumentNullException(nameof(accessor));
    }

    public string UserName => 
        _httpContext.User.FindFirstValue("CustomUserName")
        ?? throw new UnauthorizedAccessException("Missing CustomUserName claim.");

    public int UserId =>
    int.TryParse(_httpContext.User.FindFirstValue("CustomUserId"), out int userId)
        ? userId
        : throw new UnauthorizedAccessException();

    public ClaimsPrincipal User => _httpContext.User;
}
