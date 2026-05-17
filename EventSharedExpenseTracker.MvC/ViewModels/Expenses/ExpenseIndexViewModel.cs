using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseIndexViewModel
    {
        public int TripId { get; set; }

        public string? SearchString { get; set; }
        public string? CategoryFilter { get; set; }
        public bool Creator { get; set; }

        public string? NameSortParam { get; set; }
        public string? AmmSortParam { get; set; }
        public string? DateSortParam { get; set; }
        public string? CurrentSort { get; set; }

        public IEnumerable<ExpenseListItemViewModel> Expenses { get; set; } = [];
    }

    public class ExpenseListItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";

        public DateTime Date { get; set; }

        public decimal AmountSum { get; set; }

        public bool CanEdit { get; set; }

        public List<PaymentDisplayViewModel> Payments { get; set; } = [];
    }

    public class PaymentDisplayViewModel
    {
        public string ParticipantName { get; set; } = "";

        public decimal Amount { get; set; }

        public bool IsOwed { get; set; }

        public bool IsEquallyShared { get; set; }
    }
}
