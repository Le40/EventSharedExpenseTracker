using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using EventSharedExpenseTracker.Application.Expenses.DTOs;

namespace EventSharedExpenseTracker.MvC.Factories
{
    public class ExpenseIndexViewFactorycs
    {
        public static ExpenseIndexViewModel Create(
            int tripId,
            IEnumerable<ExpenseQuery> queries,
            string? sortOrder = null,
            string? searchString = null,
            string? categoryFilter = null,
            bool creator = false)
        {
            //var queries = expenses
            //    .Select(e => ExpenseMapper.MapToQuery(e, userId))
            //    .ToList();

            return new ExpenseIndexViewModel
            {
                TripId = tripId,

                SearchString = searchString,
                CategoryFilter = categoryFilter,
                Creator = creator,
                CurrentSort = sortOrder,

                NameSortParam = sortOrder == "name" ? "name_desc" : "name",
                DateSortParam = sortOrder == "date" ? "date_desc" : "date",
                AmmSortParam = sortOrder == "amount" ? "amount_desc" : "amount",

                Expenses = queries
                    .Select(ExpenseVMMapper.FromQuery)
                    .ToList(),
            };
        }
    }
}
