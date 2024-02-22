using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace EventSharedExpenseTracker.Infrastructure.Identity;

public class CustomClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser>
{
    public CustomClaimsPrincipalFactory(UserManager<ApplicationUser> userManager,IOptions<IdentityOptions> optionsAccessor): base(userManager, optionsAccessor)
    {
    }

    // This method is called only when login. It means that "the drawback   
    // of calling the database with each HTTP request" never happen.  
    public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        if (principal.Identity != null)
        {
            if (user.CustomUserId > 0)
            {
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("CustomUserId", user.CustomUserId.ToString()) });
            }
            if (!string.IsNullOrEmpty(user.CustomUserName))
            {
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("CustomUserName", user.CustomUserName) });
            }
        }

        return principal;
    }
}
