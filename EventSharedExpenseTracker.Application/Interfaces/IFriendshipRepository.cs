using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IFriendshipRepository
{
    Task<Friendship?> GetByIdAsync(int id);
    void Create(Friendship friendship);
    void Delete(Friendship friendship);
    void Update(Friendship friendship);
}
