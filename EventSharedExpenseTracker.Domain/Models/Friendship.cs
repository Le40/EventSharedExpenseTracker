
namespace EventSharedExpenseTracker.Domain.Models;

public class Friendship
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public CustomUser? User { get; set; }
    public CustomUser? Friend { get; set; }
    public bool IsConfirmed { get; set; }
}

