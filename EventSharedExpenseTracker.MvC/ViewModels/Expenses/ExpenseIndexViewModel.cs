using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.ValueObjects;
using EventSharedExpenseTracker.MvC.Common;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseIndexViewModel
    {
        public int TripId { get; set; }
        public string? SearchString { get; set; }
        public ExpenseCategory? CategoryFilter { get; set; }
        public bool Creator { get; set; }
        public string? NameSortParam { get; set; }
        public string? AmountSortParam { get; set; }
        public string? DateSortParam { get; set; }
        public string? CurrentSort { get; set; }

        public string BaseCurrencyCode { get; set; } = "EUR";

        public IEnumerable<ExpenseListItemViewModel> Expenses { get; set; } = [];

        public string EIdCreateExpense => UiIds.CreateExpense;
        public string EIdExpenseCollection => UiIds.ExpenseCollection;
    }

    public class ExpenseListItemViewModel
    {
        public int Id { get; set; }
        public bool CanUserEdit { get; set; }
        public string EIdEditExpense => UiIds.EditExpense(Id);
        public required string Name { get; set; }
        public ExpenseCategory Category { get; set; }
        public DateOnly Date { get; set; }
        public required Money TotalPaidBase { get; set; }
        public required Money TotalPaidOriginal { get; set; }
        //public string CurrencyCode { get; set; } = "EUR";

        public IEnumerable<PaymentQuery> PaidPayments { get; set; } = [];
        public IEnumerable<PaymentQuery> OwedPayments { get; set; } = [];
    }

    public class PaymentDisplayViewModel
    {
        public required string ParticipantName { get; set; }
        public decimal Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}
