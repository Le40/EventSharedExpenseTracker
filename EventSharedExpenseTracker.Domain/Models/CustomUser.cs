using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class CustomUser
{
    public int Id { get; set; }
    [StringLength(25)]
    public required string CustomUserName { get; set; }
    public ICollection<Expense> ExpensesCreated { get; } = [];
    public ICollection<Trip> TripsCreated { get; } = [];
    public ICollection<Friendship> Friends { get; } = [];
    public ICollection<TripParticipant> TripsParticipated { get; } = [];
}
