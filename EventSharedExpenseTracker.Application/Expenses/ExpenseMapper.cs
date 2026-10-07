using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.Application.Expenses
{
    public static class ExpenseMapper
    {
        public static ExpenseQuery ToQuery(Expense expense, bool canUserEdit)
        {
            var query = new ExpenseQuery
            {
                Id = expense.Id,
                CanUserEdit = canUserEdit,
                TripId = expense.TripId,
                Name = expense.Name,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description,
                CurrencyCode = expense.CurrencyCode,
            };

            foreach (var payment in expense.Payments)
            {
                query.Payments.Add(new PaymentQuery
                {
                    Id = payment.Id,
                    ParticipantId = payment.ParticipantId,
                    ParticipantName = payment.Participant.DisplayName,
                    AmountOriginal = Math.Abs(payment.AmountOriginal),
                    AmountBase = Math.Abs(payment.AmountBase),
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared
                });
            }
            return query;
        }

    }
}