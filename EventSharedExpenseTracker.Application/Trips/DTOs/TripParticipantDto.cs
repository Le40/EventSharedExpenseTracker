
namespace EventSharedExpenseTracker.Application.Trips.DTOs
{

    public record TripParticipantDto 
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public bool IsDummy { get; set; }
        public decimal PaymentSum { get; set; }
        public int PaymentCount { get; set; }
        public bool CanBeDeleted { get; set; }
    }
}
