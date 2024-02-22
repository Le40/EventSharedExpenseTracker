using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Payment
{
    public int Id { get; set; }

    [DataType(DataType.Currency)]
    [DisplayFormat(DataFormatString = "{0:C2}")]
    public double? Ammount { get; set; }
    public bool IsValid { get; set; }
    public bool IsOwed { get; set; }
    public bool IsEquallyShared { get; set; }
    //public required string UserName {  get; set; }

    public int ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public int? UserId { get; set; } // ??
    public CustomUser? User { get; set; } // ??
    public int ParticipantId { get; set; }
    public TripParticipant? Participant { get; set; }
}
