using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Validation;

public class ValidationService : IValidationService
{
    public Result ValidateTrip(TripCommand command)
    {
        var errors = new List<AppError>();

        if (command.DateFrom > command.DateTo)
        {
            errors.Add(AppErrors.Validation<Trip>("Trip can't end before it begins."));
            return Result.Fail(errors);
        }
        return Result.Success();
    }

    public Result<ExpenseCommand> ProcessForSaving(ExpenseCommand command)
    {
        // any owed payment with amount cannot be equally shared.
        NormalizeOwedInputs(command);

        var inputErrors = ValidateInputAmounts(command);
        if (inputErrors.Count>0)
            return Result<ExpenseCommand>.Fail(inputErrors);

        var sharedOwed = command.Payments.Where(p => p.IsEquallyShared).ToList();

        var totals = CalculateTotals(command, sharedOwed.Count);

        var totalErrors = ValidateTotals(totals);

        if (totalErrors.Count>0)
            return Result<ExpenseCommand>.Fail(totalErrors);

        ApplySharedOwedAmounts(sharedOwed, totals.RemainingAmountToShare);

        return Result<ExpenseCommand>.Ok(command);
    }

    private static void NormalizeOwedInputs(ExpenseCommand command)
    {
        foreach (var owed in command.Payments.Where(p => p.IsOwed))
        {
            // Manual amount wins over equal-share checkbox.
            if (owed.Amount is not null)
                owed.IsEquallyShared = false;
        }
    }

    private static List<AppError> ValidateInputAmounts(ExpenseCommand command)
    {
        var errors = new List<AppError>();

        if (command.Payments.Any(p => p.Amount < 0))
            errors.Add(AppErrors.Validation<Expense>("Amounts cannot be negative."));

        return errors;
    }

    private static ExpenseTotals CalculateTotals(
        ExpenseCommand command,
        int sharedCount)
    {
        var sumPaid = command.Payments.Where(p => !p.IsOwed).Sum(p => p.Amount ?? 0);

        var sumOwed = command.Payments
            .Where(p => p.IsOwed)
            .Sum(p => p.Amount ?? 0);
        var remainingAmountToShare = sumPaid - sumOwed;

        return new ExpenseTotals(
            SumPaid: sumPaid,
            SumOwed: sumOwed,
            RemainingAmountToShare: remainingAmountToShare,
            SharedCount: sharedCount);
    }

    private static List<AppError> ValidateTotals(ExpenseTotals totals)
    {
        var errors = new List<AppError>();

        if (totals.SumPaid == 0)
            errors.Add(AppErrors.Validation<Expense>("Paid amount can't be 0."));

        if (totals.RemainingAmountToShare < 0)
            errors.Add(AppErrors.Validation<Expense>("Spent amount is more than paid."));

        if (totals.RemainingAmountToShare > 0 && totals.SharedCount == 0)
            errors.Add(AppErrors.Validation<Expense>("Paid amount is more than spent."));

        return errors;
    }

    private static void ApplySharedOwedAmounts(
        List<PaymentCommand> sharedOwed,
        decimal remainingAmountToShare)
    {
        if (sharedOwed.Count == 0)
            return;

        var sharedAmount = remainingAmountToShare / sharedOwed.Count;

        foreach (var owed in sharedOwed)
        {
            owed.Amount = sharedAmount;
        }
    }

    private readonly record struct ExpenseTotals(
        decimal SumPaid,
        decimal SumOwed,
        decimal RemainingAmountToShare,
        int SharedCount);
}
