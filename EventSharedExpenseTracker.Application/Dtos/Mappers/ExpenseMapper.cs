using EventSharedExpenseTracker.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSharedExpenseTracker.Application.Dtos.Mappers
{
    public static class ExpenseMapper
    {
        public static ExpenseQuery MapToQuery(Expense expense, int userId)
        { 
            var canEdit = expense.CreatorId == userId;

            var query = new ExpenseQuery
            {
                Id = expense.Id,
                CanEdit = canEdit,
                Name = expense.Name,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description
            };
            foreach (var payment in expense.Payments)
            {
                query.Payments.Add(new PaymentQuery
                {
                    ParticipantName = payment.Participant!.UserName,
                    Amount = Math.Abs(payment.Ammount),
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared
                });
            }
            return query;
        }

        public static ExpenseCommand MapToCommand(Expense expense, bool canEdit)
        {
            var command = new ExpenseCommand
            {
                Id = expense.Id,
                CanEdit = canEdit,
                TripId = expense.TripId,

                Name = expense.Name,
                Date = expense.Date,
                Category = expense.Category,
                Description = expense.Description
            };

            foreach (var payment in expense.Payments)
            {
                command.Payments.Add(new PaymentCommand
                {
                    Id = payment.Id,
                    ParticipantId = payment.ParticipantId,
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared,
                    Amount = Math.Abs(payment.Ammount) 
                });
            }
            return command;
        }

        public static Expense MapToExpense(ExpenseCommand command, int tripId, int userId)
        {
            var expense = new Expense
            {
                CreatorId = userId, // resouce.CreatorId is userId from the service layer
                TripId = tripId,// resouce.TripId is tripId from the service layer

                Name = command.Name,
                Date = command.Date,
                Category = command.Category,
                Description = command.Description,
            };

            AddPayments(expense, command.Payments);
            return expense;
        }

        public static void ApplyToExpense(Expense expense, ExpenseCommand command)
        {
            expense.Name = command.Name;
            expense.Date = command.Date;
            expense.Category = command.Category;
            expense.Description = command.Description;

            ReplacePayments(expense, command.Payments);
        }

        private static void AddPayments(Expense expense, IEnumerable<PaymentCommand> paymentCommands)
        {
            foreach (var payment in paymentCommands.Where(p => p.Amount > 0))
            {
                var amount = payment.Amount!.Value;
                expense.Payments.Add(new Payment
                {
                    Ammount = payment.IsOwed ? amount * -1 : amount,
                    IsOwed = payment.IsOwed,
                    IsEquallyShared = payment.IsEquallyShared,
                    ParticipantId = payment.ParticipantId
                });
            }
            // possibly not needed, as i need amountSum mainly for show.
            expense.AmountSum = expense.Payments.Where(p => !p.IsOwed).Sum(p => p.Ammount);
        }

        private static void ReplacePayments(Expense expense, IEnumerable<PaymentCommand> paymentCommands)
        {
            expense.Payments.Clear();
            AddPayments(expense, paymentCommands);
        }
    }
}