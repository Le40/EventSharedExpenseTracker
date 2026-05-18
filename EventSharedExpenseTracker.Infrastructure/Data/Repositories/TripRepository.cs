using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Trips.DTOs;

namespace EventSharedExpenseTracker.Infrastructure.Data.Repositories;

public class TripRepository : ITripRepository
{
    private readonly ApplicationDbContext _context;

    public TripRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trip>> GetAllFromUserAsync(int userId, TripQueryOptions options)
    {
        // DEFAULT MANDATORY FILTER
        var query = _context.Trips
            .Include(t => t.Participants)
            .Where(t => t.Participants.Any(p => p.UserId == userId));

        if (!string.IsNullOrWhiteSpace(options.SearchString))
        {
            query = query.Where(e =>
            e.Name.Contains(options.SearchString));
        }

        //if (!string.IsNullOrWhiteSpace(options.Category))
        //    query = query.Where(e => e.Category == options.Category);

        query = options.SortBy switch
        {
            "name" => query.OrderBy(e => e.Name),
            "name_desc" => query.OrderByDescending(e => e.Name),
            "date" => query.OrderBy(e => e.DateFrom),
            _ => query.OrderByDescending(e => e.DateFrom),
        };

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
