using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class TripRepository : ITripRepository
{
    private readonly ApplicationDbContext _context;

    public TripRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trip>> GetAllFromUserAsync(int userId,
        Func<IQueryable<Trip>, IOrderedQueryable<Trip>> orderBy,
        params Func<IQueryable<Trip>, IQueryable<Trip>>[] filters)
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.Trips.Where(t => t.Participants.Any(p => p.UserId == userId));

        foreach (var filter in filters)
        {
            query = filter(query);
        }

        query = orderBy(query);

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<Trip?> GetByIdAsync(int id)
    {
        return await _context.Trips.FindAsync(id);
    }

    public async Task<Trip?> GetByIdWithExpenses(int id)
    {
        return await _context.Trips
            .Include(t => t.Expenses.OrderByDescending(e => e.Id))
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public void Add(Trip trip)
    {
        _context.Trips.Add(trip);
    }

    public void Update (Trip trip)
    {
        _context.Update(trip);
    }

    public void Delete(Trip trip)
    {
        _context.Trips.Remove(trip);
    }
}
