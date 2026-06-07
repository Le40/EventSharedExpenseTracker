using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.PaymentProcessing;

namespace EventSharedExpenseTracker.Application.Expenses.Commands
{
    public record ExpenseCommand
    {
        public required string Name { get; set; }
        public DateOnly Date { get; set; }
        public ExpenseCategory Category { get; set; }
        public string? Description { get; set; }
        public string CurrencyCode { get; set; } = "EUR";
        //public decimal ExchangeRateToBase { get; set; } = 1m;
        public ICollection<PaymentDraft> Payments { get; set; } = [];
    }
}

