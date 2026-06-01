using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.MvC.Common;
using System.Runtime.CompilerServices;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
{
    public class ExpenseIndexViewModel
    {
        public int TripId { get; set; }
        public string? SearchString { get; set; }
        public ExpenseCategory? CategoryFilter { get; set; }
        public bool Creator { get; set; }
        public string? NameSortParam { get; set; }
        public string? AmmSortParam { get; set; }
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
        public DateTime Date { get; set; }
        public decimal TotalPaid { get; set; }

        public IEnumerable<PaymentDisplayViewModel> PaidPayments { get; set; } = [];
        public IEnumerable<PaymentDisplayViewModel> OwedPayments { get; set; } = [];
    }

    public class PaymentDisplayViewModel
    {
        public required string ParticipantName { get; set; }
        public decimal Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}
