using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.PaymentProcessing;

namespace EventSharedExpenseTracker.Application.Expenses.DTOs
{
    public record ExpenseCommand
    {
        public required string Name { get; set; }
        public DateTime Date { get; set; }
        public ExpenseCategory Category { get; set; }
        public string? Description { get; set; }

        public ICollection<PaymentInput> Payments { get; set; } = [];
    }

    /*public record PaymentInput
    {
        public int Id { get; set; } // for updates, not needed for creates
        public int ParticipantId { get; set; }
        public decimal? Amount { get; set; }
        public bool IsOwed { get; set; }
        public bool IsEquallyShared { get; set; }
    }*/
}

