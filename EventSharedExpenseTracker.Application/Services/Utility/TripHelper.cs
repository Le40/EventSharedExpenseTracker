using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Utility;

public static class TripHelper
{
    public static Func<IQueryable<Trip>, IQueryable<Trip>> Search(string searchString)
    {
        if (searchString == null)
            return query => query;

        return query => query.Where(t =>
            t.Name.ToLower().Contains(searchString.ToLower()));
    }

    //public static Func<IQueryable<Trip>, IQueryable<Trip>> CategoryFilter(string categoryFilter)
    //{
    //    if (categoryFilter == null)
    //        return query => query;

    //   return query => query.Where(t => t.Category.ToLower().Contains(categoryFilter.ToLower()));
    //}

    public static Func<IQueryable<Trip>, IQueryable<Trip>> CreatorFilter(int creatorId)
    {
        if (creatorId <= 0)
            return query => query;

        return query => query.Where(e => e.CreatorId == creatorId);
    }


    public static Func<IQueryable<Trip>, IOrderedQueryable<Trip>> GetOrderByExpression(string sortOrder)
    {
        switch (sortOrder)
        {
            case "name":
                return q => q.OrderBy(e => e.Name);
            case "name_desc":
                return q => q.OrderByDescending(e => e.Name);
            case "date":
                return q => q.OrderBy(e => e.DateFrom);
            case "date_desc":
                return q => q.OrderByDescending(e => e.DateFrom);
            default:
                return q => q.OrderByDescending(e => e.Id);
        }
    }
}
