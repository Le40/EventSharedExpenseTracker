using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IUserRepository
{
    Task<List<CustomUser>> GetAllAsync(int userId,
        params Func<IQueryable<CustomUser>, IQueryable<CustomUser>>[] filters);

    // this below, simpler version for testing, and also after going back i dont know why so complicted, maybe i will remember.
    //Task<List<CustomUser>> GetAllAsync();

    Task<CustomUser?> GetUserWithFriends(int id);
    Task<CustomUser?> GetByIdAsync(int id);
    void Create(CustomUser customUser);
    void Delete(CustomUser customUser);
    void Update(CustomUser customUser);
}
