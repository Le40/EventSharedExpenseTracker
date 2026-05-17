using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface IFriendService
{
    Task<Result<List<Friendship>>> Index();
    Task<Result<List<CustomUser>>> Search (string searchString);
    Task<Result<Friendship>> Invite(int friendId);
    Task<Result<Friendship>> Accept(int friendshipId);
    Task<Result> Decline(int friendshipId);
    Task<Result> Delete(int friendshipId);
}
