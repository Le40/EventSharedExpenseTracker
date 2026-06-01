using EventSharedExpenseTracker.Application.Common;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.PaymentProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    ParticipantName = TripParticipantMapper.GetDisplayName(payment.Participant),
                    AmountOriginal = Math.Abs(payment.AmountOriginal),
                    AmountBase = Math.Abs(payment.AmountBase),
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared
                });
            }
            return query;
        }

        public static Expense ToExpense(ExpenseCommand command, ExpenseCreationContext context)
        {
            // FOR CREATE ONLY
            var expense = new Expense
            {
                CreatorId = context.UserId, // resouce.CreatorId is userId from the service layer
                TripId = context.TripId,// resouce.TripId is tripId from the service layer
                BaseCurrencyCode = context.TripBaseCurrencyCode,
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

            //ReplacePayments(expense, command.Payments);
        }

        private static void AddPayments(Expense expense, IEnumerable<PaymentInput> paymentCommands)
        {
            foreach (var payment in paymentCommands.Where(p => p.Amount > 0))
            {
                var amount = payment.Amount!.Value;
                expense.Payments.Add(new Payment
                {
                    AmountBase = payment.IsOwed ? amount * -1 : amount,
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared,
                    ParticipantId = payment.ParticipantId
                });
            }
            // possibly not needed, as i need amountSum mainly for show.
            //expense.AmountSum = expense.Payments.Where(p => !p.IsOwed).Sum(p => p.Ammount);
        }

        private static void ReplacePayments(Expense expense, IEnumerable<PaymentInput> paymentCommands)
        {
            expense.Payments.Clear();
            AddPayments(expense, paymentCommands);
        }
    }
}