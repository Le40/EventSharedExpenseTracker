using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Trip
{
    public int Id { get; set; }
    [StringLength(25)]
    public required string Name { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime DateTo { get; set; }
    public int? CreatorId { get; set; }
    public CustomUser? Creator { get; set; }
    public string? ImagePath { get; set; }

    public ICollection<TripParticipant> Participants { get; } = [];
    public ICollection<Expense> Expenses { get; } = [];
}
