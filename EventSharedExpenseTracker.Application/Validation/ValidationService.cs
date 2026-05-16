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


        /*var appValResult = new AppValidationResult();
        if (trip.DateFrom > trip.DateTo)
            appValResult.AddError("Trip can't end before it begins.", nameof(trip.DateTo));
        //return new AppValidationResult { IsValid = false, ErrorMessage = "Trip can't end before it begins." };

        //return new AppValidationResult { IsValid = true };
        return appValResult;
    }*/

    /*public AppValidationResult ValidateExpense(Expense expense)
    {
        var appValResult = new AppValidationResult();
        ValidatePayments(expense);

        var paymentsPaid = expense.Payments.Where(p => !p.IsOwed);
        var paymentsOwed = expense.Payments.Where(p => p.IsOwed);

        decimal sumPaid = paymentsPaid.Sum(p => p.Ammount);
        decimal sumOwed = paymentsOwed.Sum(p => p.Ammount);
        decimal sumDifference = sumPaid - sumOwed;

        int sharedCount = CountAndUpdateSharedPayments(sumDifference, paymentsOwed);

        paymentsOwed.ToList().ForEach(p => p.Ammount *= -1); // set owed to be negative, cause from form it is positive.

        if (sumPaid == 0)
            appValResult.AddError("Paid amount cant be 0.");
        //return new AppValidationResult { IsValid = false, ErrorMessage = "Paid amount cant be 0." };

        if (sumDifference < 0)
            appValResult.AddError("Spent amount is more than Paid.");
            //return new AppValidationResult { IsValid = false, ErrorMessage = "Spent amount is more than Paid." };

        if (sumDifference > 0 && sharedCount == 0)
            appValResult.AddError("Paid amount is more than Spent.");
            //return new AppValidationResult { IsValid = false, ErrorMessage = "Paid amount is more than Spent." };

        expense.AmountSum = sumPaid;
        return appValResult;
        //return new AppValidationResult { IsValid = true };
    }*/


    /*public Result<Expense> ProcessForSaving(Expense expense)
    {
        var paidPayments = expense.Payments.Where(p => !p.IsOwed).ToList();
        var owedPayments = expense.Payments.Where(p => p.IsOwed).ToList();
        // only zero amount and equally shared payments are considered as equally shared. 
        // because user can set amount and also mark as equally shared, in this case its better to keep the amount.
        var sharedPayments = owedPayments.Where(p.IsEquallyShared && p.Ammount == null).ToList();

        var totals = CalculateTotals(paidPayments, owedPayments, sharedPayments);

        var validationErrors = ValidateExpenseTotals(totals);

        if (validationErrors.Any())
            return Result<Expense>.Fail(validationErrors);

        ApplySharedOwedAmounts(sharedPayments, totals.SumDifference);

        ConvertOwedAmountsToNegative(owedPayments);

        expense.AmountSum = totals.SumPaid;

        return Result<Expense>.Ok(expense);
    }*/

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

    /*private static void ValidatePayments(Expense expense)
    {
        foreach (var p in expense.Payments)
        {
            // set 0 to null
            p.Ammount = (p.Ammount == 0) ? null : p.Ammount;
            // all not null are valid, but all valid payments are not null
            p.IsValid = (p.Ammount) != null ? true : p.IsValid;
        }
    }


    private static int CountAndUpdateSharedPayments(decimal sumDifference, IEnumerable<Payment> paymentsOwed)
    {
        var sharedPayments = paymentsOwed.Where(p => p.IsValid && p.Ammount == null).ToList();
        int sharedCount = sharedPayments.Count;

        if (sharedCount > 0)
        {
            decimal sharedAmount = sumDifference / sharedCount;
            foreach (var payment in sharedPayments)
            {
                payment.Ammount = sharedAmount;
                payment.IsEquallyShared = (sharedCount != 1);
            }
        }

        return sharedCount;
    }*/
}
