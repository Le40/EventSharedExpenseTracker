
namespace EventSharedExpenseTracker.Domain.PaymentProcessing
{
    public class PaymentDraft
    {
        public int ParticipantId { get; init; }
        public decimal? UserEnteredAmount { get; set; }
        public bool IsOwed { get; init; }
        public bool IsEquallyShared { get; set; }
    }
}
