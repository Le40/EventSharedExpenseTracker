using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Services.Utility;

public static class FriendHelper
{
    public static Func<IQueryable<CustomUser>, IQueryable<CustomUser>> Search(string searchString)
    {
        if (searchString == null)
            return query => query;

        return query => query.Where(e =>
            e.CustomUserName.ToLower().Contains(searchString.ToLower())); //||
            //e.Email.ToLower().Contains(searchString.ToLower()));
    }

    //public static Func<IQueryable<Expense>, IQueryable<Expense>> FriendFilter(string categoryFilter)
    //{
    //    if (categoryFilter == null)
    //        return query => query;

    //    return query => query.Where(e => e.Category.ToLower().Contains(categoryFilter.ToLower()));
    //}

}
