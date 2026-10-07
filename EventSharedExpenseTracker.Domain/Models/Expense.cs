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
    public Guid? OfflineClientId { get; set; }

    public ICollection<Payment> Payments { get; } = [];

    public bool HasCreator => CreatorId is not null;

    public bool IsCreatedBy(int userId)
    {
        return CreatorId == userId;
    }

    public static DomainResult<Expense> Create(
        string name,
        DateOnly date,
        ExpenseCategory category,
        string? description,
        string currencyCode,
        int tripId,
        int userId,
        decimal exchangeRateToBase,
        Guid? offlineClientId,
        ICollection<Payment> payments)
    {
        if (exchangeRateToBase <= 0m)
            return DomainErrors.Validation<Expense>("Exchange rate must be greater than zero.");

        var paymentList = payments.ToList();

        var validationResult = ValidatePayments(paymentList);

        if (!validationResult.IsSuccess)
            return DomainResult<Expense>.Fail(validationResult.Errors);

        var expense = new Expense
        {
            Name = name,
            Date = date,
            Category = category,
            Description = description,
            CurrencyCode = currencyCode,
            TripId = tripId,
            CreatorId = userId,
            ExchangeRateToBase = exchangeRateToBase,
            OfflineClientId = offlineClientId
        };

        foreach (var payment in paymentList)
            expense.Payments.Add(payment);

        return expense;
    }

    public DomainResult Update(
      string name,
      DateOnly date,
      ExpenseCategory category,
      string? description,
      string currencyCode,
      decimal exchangeRateToBase,
      ICollection<Payment> payments)
    {
        if (exchangeRateToBase <= 0m)
            return DomainErrors.Validation<Expense>(
                "Exchange rate must be greater than zero.");

        var paymentList = payments.ToList();

        var validationResult = ValidatePayments(paymentList);

        if (!validationResult.IsSuccess)
            return validationResult;

        Name = name;
        Date = date;
        Category = category;
        Description = description;
        CurrencyCode = currencyCode;
        ExchangeRateToBase = exchangeRateToBase;

        Payments.Clear();

        foreach (var payment in paymentList)
            Payments.Add(payment);

        return DomainResult.Ok();
    }

    private static DomainResult ValidatePayments(
        IReadOnlyCollection<Payment> payments)
    {
        if (!payments.Any(p => !p.IsOwed))
            return DomainErrors.Validation<Expense>(
                "Expense must have at least one payer.");

        if (!payments.Any(p => p.IsOwed))
            return DomainErrors.Validation<Expense>(
                "Expense must have at least one owed participant.");

        if (payments.Sum(p => p.AmountBase) != 0m)
            return DomainErrors.Validation<Expense>(
                "Paid and owed totals must match.");

        return DomainResult.Ok();
    }
}