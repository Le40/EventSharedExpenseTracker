using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Trip
{
    public int Id { get; set; }
    [StringLength(25, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public required string Name { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
    public DateTime DateFrom { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
    public DateTime DateTo { get; set; }
    public required int CreatorId { get; set; }
    public CustomUser? Creator { get; set; }
    public string? ImagePath { get; set; }

    public List<TripParticipant> Participants { get; } = new();
    public List<Expense> Expenses { get; } = new();
}
