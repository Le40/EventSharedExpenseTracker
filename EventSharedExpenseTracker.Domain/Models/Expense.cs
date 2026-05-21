using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Result;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;



public class Expense
{
    public int Id { get; set; }
    [StringLength(50)]
    public required string Name { get; set; }
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Now;
    [StringLength(25)]
    public ExpenseCategory Category { get; set; }
    [StringLength(140)]
    public string? Description { get; set; }
    public int? CreatorId { get; set; }
    public CustomUser? Creator { get; set; }
    public int TripId { get; set; }
    public Trip? Trip { get; set; }

    public ICollection<Payment> Payments { get; } = [];

    public bool HasCreator => CreatorId is not null;

    public bool IsCreatedBy(int userId)
    {
        return CreatorId == userId;
    }

    public void SetPayments(IEnumerable<Payment> payments)
    {
        Payments.Clear();

        foreach (var payment in payments)
        {
            Payments.Add(payment);
        }
    }
}