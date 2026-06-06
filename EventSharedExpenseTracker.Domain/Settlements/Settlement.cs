
namespace EventSharedExpenseTracker.Domain.Settlements
{
    public record Settlement
    {
        public int FromParticipantId { get; init; }
        public string FromParticipantName { get; init; } = "";

        public int ToParticipantId { get; init; }
        public string ToParticipantName { get; init; } = "";

        public decimal Amount { get; init; }
    }
}
