using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public interface IFriendService
{
    Task<ServiceResult<List<Friendship>>> Index();
    Task<ServiceResult<List<CustomUser>>> Search (string searchString);
    Task<ServiceResult<Friendship>> Invite(int friendId);
    Task<ServiceResult<Friendship>> Accept(int friendshipId);
    Task<ServiceResult<Friendship>> Decline(int friendshipId);
    Task<ServiceResult<Friendship>> Delete(int friendshipId);
}
