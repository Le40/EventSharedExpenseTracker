using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.Result;

namespace EventSharedExpenseTracker.Domain.PaymentProcessing{

    public static class ExpenseProcessor
    {
        public static DomainResult<ICollection<Payment>> ProcessForSaving(ICollection<PaymentDraft> drafts, decimal exchangeRateToBase)
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
                .Where(p => p.UserEnteredAmount > 0m)
                .Select(p =>
                {
                    var amount = Math.Round(p.IsOwed ? -p.UserEnteredAmount!.Value : p.UserEnteredAmount!.Value,2);

                    return new Payment
                    {
                        ParticipantId = p.ParticipantId,
                        AmountOriginal = amount,
                        AmountBase = Math.Round(amount * exchangeRateToBase, 2),
                        IsOwed = p.IsOwed,
                        IsEquallyShared = p.IsEquallyShared
                    };
                })
                .ToList();

            if (payments.Count == 0)
                return DomainErrors.NoPayments;

            RecalculateSharedAmountsBaseAndApplyRemainder(payments);

            if (payments.Sum(p => p.AmountBase) != 0)
                return DomainErrors.Validation<Expense>("Paid and owed totals must match.");

            return payments;
        }


        private static void NormalizeOwedInputs(ICollection<PaymentDraft> paymentDrafts)
        {
            foreach (var owed in paymentDrafts.Where(p => p.IsOwed))
            {
                // Manual amount wins over equal-share checkbox.
                if (owed.UserEnteredAmount > 0m)
                    owed.IsEquallyShared = false;
            }
        }

        private static List<DomainError> ValidateInputAmounts(ICollection<PaymentDraft> paymentDrafts)
        {
            var errors = new List<DomainError>();
            if (paymentDrafts.Any(p => p.UserEnteredAmount < 0))
                errors.Add(DomainErrors.Validation<Payment>("Payment value cant be negative"));

            return errors;
        }

        private static ExpenseTotals CalculateTotals(
            ICollection<PaymentDraft> paymentDrafts,
            int sharedCount)
        {
            var sumPaid = paymentDrafts.Where(p => !p.IsOwed).Sum(p => p.UserEnteredAmount ?? 0m);

            var sumOwed = paymentDrafts
                .Where(p => p.IsOwed)
                .Sum(p => p.UserEnteredAmount ?? 0m);

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
            ICollection<PaymentDraft> sharedOwed,
            decimal remainingAmountToShare)
        {
            if (sharedOwed.Count == 0)
                return;

            var sharedAmount = remainingAmountToShare / sharedOwed.Count;

            foreach (var owed in sharedOwed)
            {
                owed.UserEnteredAmount = sharedAmount;
            }
        }

        private static void RecalculateSharedAmountsBaseAndApplyRemainder(
            ICollection<Payment> payments)
        {
            var paidSum = payments
                .Where(p => !p.IsOwed)
                .Sum(p => p.AmountBase);

            var manualOwedSum = payments
                .Where(p => p.IsOwed && !p.IsEquallyShared)
                .Sum(p => Math.Abs(p.AmountBase));

            var sharedPayments = payments
                .Where(p => p.IsOwed && p.IsEquallyShared)
                .ToList();

            if (sharedPayments.Count == 0)
                return;

            var amountToShare = -(paidSum - manualOwedSum);

            var sharedAmount = Math.Round(
                amountToShare / sharedPayments.Count,
                2,
                MidpointRounding.AwayFromZero);

            foreach (var payment in sharedPayments)
            {
                payment.AmountBase = sharedAmount;
            }

            var sharedSum = sharedPayments.Sum(p => p.AmountBase);
            var remainder = amountToShare - sharedSum;

            sharedPayments.Last().AmountBase += remainder;
        }

        private readonly record struct ExpenseTotals(
            decimal SumPaid,
            decimal SumOwed,
            decimal RemainingAmountToShare,
            int SharedCount);

    }
}
