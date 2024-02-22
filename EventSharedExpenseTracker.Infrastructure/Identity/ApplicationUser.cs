using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    [StringLength(10, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public required string CustomUserName { get; set; }
    public int CustomUserId { get; set; }
    public CustomUser? CustomUser { get; set; }
}
