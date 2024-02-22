
namespace EventSharedExpenseTracker.Domain.Models;

public class TripParticipant
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public int? UserId { get; set; }
    public CustomUser? User { get; set; }
    public int TripId { get; set; }
    public Trip? Trip { get; set; }

    public List<Payment> Payments { get; set; } = new();
}
