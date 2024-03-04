using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Expense
{
    public int Id { get; set; }

    [StringLength(25, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public string Name { get; set; }

    [DataType(DataType.Date)]
    //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}")]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "The Category field is required.")]
    public string Category { get; set; }
    [StringLength(80, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public string? Description { get; set; }

    public int? CreatorId { get; set; }
    public CustomUser? Creator { get; set; }

    public int TripId { get; set; }
    public Trip? Trip { get; set; }

    public List<Payment> Payments { get; set; } = new();

    public double? AmountSum { get; set; }

    public static List<string> Categories { get; } = new List<string>
    {
        "FOOD",
        "TRAVEL",
        "PROGRAM",
        "ROOM",
        "OTHER"
    };
}
