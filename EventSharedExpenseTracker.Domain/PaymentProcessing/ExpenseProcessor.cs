using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.Result;

namespace EventSharedExpenseTracker.Domain.PaymentProcessing{

    public static class ExpenseProcessor
    {
        public static DomainResult<ICollection<Payment>> ProcessForSaving(ICollection<PaymentInput> drafts)
        {
            // any owed payment with amount cannot be equally shared.
            NormalizeOwedInputs(drafts);

            var inputErrors = ValidateInputAmounts(drafts);
            if (inputErrors.Count > 0)
                return inputErrors;

            var sharedOwed = drafts
                .Where(p => p.IsOwed && p.IsEquallyShared)
                .ToList();

            var totals = CalculateTotals(drafts, sharedOwed.Count);

            var totalErrors = ValidateTotals(totals);

            if (totalErrors.Count > 0)
                return totalErrors;

            ApplySharedOwedAmounts(sharedOwed, totals.RemainingAmountToShare);

            var payments = drafts
                .Where(p => p.Amount > 0m)
                .Select(p => new Payment
                {
                    ParticipantId = p.ParticipantId,
                    Amount = p.IsOwed ? -p.Amount!.Value : p.Amount!.Value,
                    IsOwed = p.IsOwed,
                    IsEquallyShared = p.IsEquallyShared
                })
                .ToList();

            if (payments.Count == 0)
                return DomainErrors.NoPayments;

            return payments;
        }

        private static void NormalizeOwedInputs(ICollection<PaymentInput> paymentDrafts)
        {
            foreach (var owed in paymentDrafts.Where(p => p.IsOwed))
            {
                // Manual amount wins over equal-share checkbox.
                if (owed.Amount > 0m)
                    owed.IsEquallyShared = false;
            }
        }

        private static List<DomainError> ValidateInputAmounts(ICollection<PaymentInput> paymentDrafts)
        {
            var errors = new List<DomainError>();
            if (paymentDrafts.Any(p => p.Amount < 0))
                errors.Add(DomainErrors.Validation<Payment>("Payment value cant be negative"));

            return errors;
        }

        private static ExpenseTotals CalculateTotals(
            ICollection<PaymentInput> paymentDrafts,
            int sharedCount)
        {
            var sumPaid = paymentDrafts.Where(p => !p.IsOwed).Sum(p => p.Amount ?? 0m);

            var sumOwed = paymentDrafts
                .Where(p => p.IsOwed)
                .Sum(p => p.Amount ?? 0m);

            var remainingAmountToShare = sumPaid - sumOwed;

            return new ExpenseTotals(
                SumPaid: sumPaid,
                SumOwed: sumOwed,
                RemainingAmountToShare: remainingAmountToShare,
                SharedCount: sharedCount);
        }

        private static List<DomainError> ValidateTotals(ExpenseTotals totals)
        {
            var errors = new List<DomainError>();

            if (totals.SumPaid == 0)
                errors.Add(DomainErrors.Validation<Expense>("Paid amount can't be 0."));

            if (totals.RemainingAmountToShare < 0)
                errors.Add(DomainErrors.Validation<Expense>("Spent amount is more than paid."));

            if (totals.RemainingAmountToShare > 0 && totals.SharedCount == 0)
                errors.Add(DomainErrors.Validation<Expense>("Paid amount is more than spent."));

            return errors;
        }

        private static void ApplySharedOwedAmounts(
            ICollection<PaymentInput> sharedOwed,
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
}
