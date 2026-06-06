
namespace EventSharedExpenseTracker.Domain.Settlements
{
    public record ParticipantBalance
    {
        public int ParticipantId { get; init; }
        public string ParticipantName { get; init; } = "";
        public decimal Balance { get; init; }
    }
}
