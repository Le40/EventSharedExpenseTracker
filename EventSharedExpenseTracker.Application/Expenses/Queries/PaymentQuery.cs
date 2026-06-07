
namespace EventSharedExpenseTracker.Application.Expenses.Queries
{
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
