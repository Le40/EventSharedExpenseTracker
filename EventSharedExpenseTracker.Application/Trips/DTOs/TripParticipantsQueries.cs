
namespace EventSharedExpenseTracker.Application.Trips.DTOs
{
    public record TripParticipantQuery
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
    }

    public record TripParticipantDetailsQuery : TripParticipantQuery
    {
        public bool IsDummy { get; set; }
        public decimal PaymentSum { get; set; }

        public int PaymentCount { get; set; }
    }

    public record TripParticipantsQuery
    {
        public int TripId { get; init; }
        public string BaseCurrencyCode { get; init; } = "EUR";
        public List<TripParticipantQuery> Participants { get; init; } = [];
    }
}
