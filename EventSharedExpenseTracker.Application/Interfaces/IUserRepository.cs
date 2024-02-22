using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IUserRepository
{
    Task<List<CustomUser>> GetAllAsync(int userId,
        params Func<IQueryable<CustomUser>, IQueryable<CustomUser>>[] filters);
    Task<CustomUser?> GetByIdAsync(int id);
    void Create(CustomUser customUser);
    void Delete(CustomUser customUser);
    void Update(CustomUser customUser);
}
