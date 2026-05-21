
namespace EventSharedExpenseTracker.Domain.Models;

public class TripParticipant
{
    public int Id { get; set; }
    public required string DisplayName { get; set; }
    public int? UserId { get; set; }
    public CustomUser? User { get; set; }
    public int TripId { get; set; }
    public Trip? Trip { get; set; }
    public ICollection<Payment> Payments { get; } = [];

    public bool IsRegisteredUser => UserId != null;
    public bool HasPayments => Payments.Any();
}
