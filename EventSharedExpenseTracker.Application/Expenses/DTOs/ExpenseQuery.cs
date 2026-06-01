using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Expenses.DTOs
{
    public record ExpenseQuery
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public bool CanUserEdit { get; set; }

        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public ExpenseCategory Category { get; set; }
        public string? Description { get; set; }
        public string CurrencyCode { get; set; } = "EUR";
        public ICollection<PaymentQuery> Payments { get; set; } = [];
    }

    public record PaymentQuery
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public required string ParticipantName { get; set; }
        public decimal AmountOriginal { get; set; }
        public decimal AmountBase { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }
}

