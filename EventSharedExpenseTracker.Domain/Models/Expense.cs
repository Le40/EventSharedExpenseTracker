using EventSharedExpenseTracker.Domain.Constants;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Result;
using System.ComponentModel.DataAnnotations;

namespace EventSharedExpenseTracker.Domain.Models;

public class Expense
{
    public int Id { get; set; }
    [StringLength(ExpenseConstants.NameMaxLength)]
    public required string Name { get; set; }
    [DataType(DataType.Date)]
    public DateOnly Date { get; set; }
    [StringLength(ExpenseConstants.CategoryMaxLength)]
    public ExpenseCategory Category { get; set; }
    [StringLength(ExpenseConstants.DescriptionMaxLength)]
    public string? Description { get; set; }
    public int? CreatorId { get; set; }
    public CustomUser? Creator { get; set; }
    public int TripId { get; set; }
    public Trip? Trip { get; set; }

    [StringLength(3)]
    public string CurrencyCode { get; set; } = "EUR";
    //public string BaseCurrencyCode { get; set; } = "EUR";
    public decimal ExchangeRateToBase{ get; set; } = 1m; //maybe also not needed, good to updates of expense without changes to date and curency, no need to call db.

    public ICollection<Payment> Payments { get; } = [];

    public bool HasCreator => CreatorId is not null;

    public bool IsCreatedBy(int userId)
    {
        return CreatorId == userId;
    }

    public DomainResult SetPayments(IEnumerable<Payment> payments)
    {
        var list = payments.ToList();

        if (!list.Any(p => !p.IsOwed))
            return DomainErrors.Validation<Expense>("Expense must have at least one payer.");

        if (!list.Any(p => p.IsOwed))
            return DomainErrors.Validation<Expense>("Expense must have at least one owed participant.");

        if (list.Sum(p=>p.AmountBase) != 0)
            return DomainErrors.Validation<Expense>("Paid and owed totals must match.");

        Payments.Clear();

        foreach (var payment in list)
            Payments.Add(payment);

        return DomainResult.Ok();
    }
}