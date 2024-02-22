using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Validation;

public class ValidationService : IValidationService
{
    public ValidationResult ValidateTrip(Trip trip)
    {
        if (trip.DateFrom > trip.DateTo)
            return new ValidationResult { IsValid = false, ErrorMessage = "Trip can't end before it begins." };

        return new ValidationResult { IsValid = true };
    }

    public ValidationResult ValidateExpense(Expense expense)
    {
        ValidatePayments(expense);

        var paymentsPaid = expense.Payments.Where(p => !p.IsOwed);
        var paymentsOwed = expense.Payments.Where(p => p.IsOwed);

        double sumPaid = paymentsPaid.Sum(p => p.Ammount ?? 0);
        double sumOwed = paymentsOwed.Sum(p => p.Ammount ?? 0);
        double sumDifference = sumPaid - sumOwed;

        int sharedCount = CountAndUpdateSharedPayments(sumDifference, paymentsOwed);

        paymentsOwed.ToList().ForEach(p => p.Ammount *= -1); // set owed to be negative, cause from form it is positive.

        if (sumPaid == 0)
            return new ValidationResult { IsValid = false, ErrorMessage = "Paid amount cant be 0." };

        if (sumDifference < 0)
            return new ValidationResult { IsValid = false, ErrorMessage = "Spent amount is more than Paid." };

        if (sumDifference > 0 && sharedCount == 0)
            return new ValidationResult { IsValid = false, ErrorMessage = "Paid amount is more than Spent." };

        expense.AmountSum = sumPaid;
        return new ValidationResult { IsValid = true };
    }

    private static void ValidatePayments(Expense expense)
    {
        foreach (var p in expense.Payments)
        {
            // set 0 to null
            p.Ammount = (p.Ammount == 0) ? null : p.Ammount;
            // all not null are valid, but all valid payments are not null
            p.IsValid = (p.Ammount) != null ? true : p.IsValid;
        }
    }


    private static int CountAndUpdateSharedPayments(double sumDifference, IEnumerable<Payment> paymentsOwed)
    {
        var sharedPayments = paymentsOwed.Where(p => p.IsValid && p.Ammount == null).ToList();
        int sharedCount = sharedPayments.Count;

        if (sharedCount > 0)
        {
            double sharedAmount = sumDifference / sharedCount;
            foreach (var payment in sharedPayments)
            {
                payment.Ammount = sharedAmount;
                payment.IsEquallyShared = (sharedCount != 1);
            }
        }

        return sharedCount;
    }
}
