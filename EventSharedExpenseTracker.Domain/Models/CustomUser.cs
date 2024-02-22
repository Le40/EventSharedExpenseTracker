using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class CustomUser
{
    //public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Id { get; set; }

    [StringLength(10, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public required string CustomUserName { get; set; }

    //public string ApplicationUserId { get; set; }

    public List<Expense> ExpensesCreated { get; } = new();
    public List<Trip> TripsCreated { get; } = new();
    public List<Payment> Payments { get; } = new();
    public List<Friendship> Friends { get; } = new();
    public List<TripParticipant> TripsParticipated { get; } = new();
}
