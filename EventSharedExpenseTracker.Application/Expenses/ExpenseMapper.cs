using EventSharedExpenseTracker.Application.Expenses.Commands;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Models;
using Mapster;

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

        public static Expense FromCommand(ExpenseCommand command, ExpenseCreationContext context)
        {
            // FOR CREATE ONLY
            var expense = new Expense
            {
                CreatorId = context.UserId, // resouce.CreatorId is userId from the service layer
                TripId = context.TripId,// resouce.TripId is tripId from the service layer
                //BaseCurrencyCode = context.TripBaseCurrencyCode,
                ExchangeRateToBase = context.ExchangeRateToBase,

                Name = command.Name,
                Date = command.Date,
                Category = command.Category,
                Description = command.Description,
                CurrencyCode = command.CurrencyCode,
            };

            //AddPayments(expense, command.Payments);
            return expense;
        }

        public static void ApplyToExpense(Expense expense, ExpenseCommand command, decimal exchangeRate)
        {
            // FOR UPDATE ONLY
            expense.Name = command.Name;
            expense.Date = command.Date;
            expense.Category = command.Category;
            expense.Description = command.Description;
            expense.CurrencyCode = command.CurrencyCode;
            expense.ExchangeRateToBase = exchangeRate;
        }


    }
}