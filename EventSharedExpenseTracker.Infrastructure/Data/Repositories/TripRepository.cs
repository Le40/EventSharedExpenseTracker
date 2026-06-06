using Microsoft.EntityFrameworkCore;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Data.DbContexts;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Settlements;

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
            query = query.Where(t =>
            t.Name.Contains(options.SearchString));
        }

        //if (options.Category.HasValue)
        //   query = query.Where(t => t.Category == options.Category.Value);

        query = options.SortBy switch
        {
            "name" => query.OrderBy(t => t.Name),
            "name_desc" => query.OrderByDescending(t => t.Name),
            "date" => query.OrderBy(t => t.DateFrom),
            _ => query.OrderByDescending(t => t.DateFrom),
        };

        return await query
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Trip?> GetByIdAsync(int id)
    {
        return await _context.Trips.FindAsync(id);
    }

    public async Task<Trip?> GetByIdWithExpensesAsync(int id)
    {
        return await _context.Trips
            .AsSplitQuery()
            .Include(t => t.Expenses.OrderByDescending(e => e.Id))
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<List<ParticipantBalance>> GetParticipantBalancesAsync(int tripId)
    {
        return await _context.TripParticipants
            .AsNoTracking()
            .Where(p => p.TripId == tripId)
            .Select(p => new ParticipantBalance
            {
                ParticipantId = p.Id,
                ParticipantName = p.DisplayName,
                Balance = p.Payments.Sum(x => x.AmountBase)
            })
            .ToListAsync();
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
        _context.Payments.RemoveRange(
            trip.Expenses.SelectMany(e => e.Payments));

        _context.Expenses.RemoveRange(trip.Expenses);
        _context.TripParticipants.RemoveRange(trip.Participants);
        _context.Trips.Remove(trip);
    }
}
