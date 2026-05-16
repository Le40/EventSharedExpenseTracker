using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseIndexViewModel
    {
        public IEnumerable<ExpenseListItemViewModel> Expenses { get; set; } = [];
        public int TripId { get; set; }

        public string? SearchString { get; set; }
        public string? CategoryFilter { get; set; }
        public bool Creator { get; set; }

        public string? NameSortParam { get; set; }
        public string? AmmSortParam { get; set; }
        public string? DateSortParam { get; set; }
        public string? CurrentSort { get; set; }
    }
}
