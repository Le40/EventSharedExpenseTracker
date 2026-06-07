using EventSharedExpenseTracker.Application.Friends;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<List<CustomUser>> GetAllAsync(int userId, FriendshipQueryOptions options);

    // this below, simpler version for testing, and also after going back i dont know why so complicted, maybe i will remember.
    //Task<List<CustomUser>> GetAllAsync();

    Task<CustomUser?> GetUserWithFriends(int id);
    Task<CustomUser?> GetByIdAsync(int id);
    void Create(CustomUser customUser);
    void Delete(CustomUser customUser);
    Task UpdateAndSyncAsync(CustomUser customUser);
}
