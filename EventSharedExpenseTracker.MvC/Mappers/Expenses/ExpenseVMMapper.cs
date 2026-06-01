using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.PaymentProcessing;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;

namespace EventSharedExpenseTracker.MvC.Mappers.Expenses
{
    public static class ExpenseVMMapper
    {
        public static ExpenseListItemViewModel FromQuery(ExpenseQuery query)
        {
            var paidPayments = query.Payments.Where(p => !p.IsOwed).Select(p => new PaymentDisplayViewModel
            {
                ParticipantName = p.ParticipantName,
                Amount = p.AmountBase,
                IsOwed = p.IsOwed,
                IsEquallyShared = p.IsEquallyShared
            }).ToList();

            var owedPayments = query.Payments.Where(p => p.IsOwed).Select(p => new PaymentDisplayViewModel
            {
                ParticipantName = p.ParticipantName,
                Amount = p.AmountBase,
                IsOwed = p.IsOwed,
                IsEquallyShared = p.IsEquallyShared
            }).ToList();


            return new ExpenseListItemViewModel
            {
                Id = query.Id,
                Name = query.Name,
                Category = query.Category,
                Date = query.Date,
                CanUserEdit = query.CanUserEdit,
                PaidPayments = paidPayments,
                OwedPayments = owedPayments,
                TotalPaid = paidPayments.Sum(p => p.Amount)
            };
        }
        public static ExpenseCommand ToCommand(ExpenseFormViewModel model) //, int userId
        {
            var expenseCommand = new ExpenseCommand
            {
                Name = model.Name,
                Date = model.Date,
                Category = model.Category.Value,
                Description = model.Description,
                CurrencyCode = model.CurrencyCode   
            };

            foreach (var participant in model.Participants)
            { 
                if (participant.PaidAmount > 0) // validated in viewmodel 
                {
                    expenseCommand.Payments.Add(new PaymentInput
                    {
                        ParticipantId = participant.ParticipantId,
                        Amount = participant.PaidAmount.Value,
                        IsOwed = false,
                        IsEquallyShared = false
                    });
                }

                if (participant.IsOwedSelected || participant.OwedAmount > 0) // validated in viewmodel
                {
                    expenseCommand.Payments.Add(new PaymentInput
                    {
                        ParticipantId = participant.ParticipantId,
                        Amount = participant.OwedAmount,
                        IsOwed = true,
                        IsEquallyShared = participant.IsOwedSelected
                    });
                }
            }

            return expenseCommand;
        }

        public static ExpenseFormViewModel FromQuery(
            ExpenseQuery command,
            IEnumerable<TripQueryParticipant> tripParticipants)
        {
            var paidByParticipant = command.Payments
                .Where(p => !p.IsOwed)
                .ToDictionary(p => p.ParticipantId);

            var owedByParticipant = command.Payments
                .Where(p => p.IsOwed)
                .ToDictionary(p => p.ParticipantId);

            var expenseViewModel = new ExpenseFormViewModel
            {
                Id = command.Id,
                TripId = command.TripId,
                CanUserEdit = command.CanUserEdit,

                Name = command.Name,
                Date = command.Date,
                Category = command.Category,
                Description = command.Description,
                CurrencyCode = command.CurrencyCode,
                CurrencyOptions = CurrencySelectList.Get("EUR")
            };

            foreach (var participant in tripParticipants)
            {
                paidByParticipant.TryGetValue(participant.Id, out var paid);
                owedByParticipant.TryGetValue(participant.Id, out var owed);

                expenseViewModel.Participants.Add(
                    new ExpenseFormParticipantViewModel
                    {
                        ParticipantId = participant.Id,
                        ParticipantName = participant.DisplayName,

                        PaidPaymentId = paid?.Id,
                        PaidAmount = paid?.AmountOriginal,

                        OwedPaymentId = owed?.Id,
                        // every owed payment is selected in form.
                        IsOwedSelected = owed != null,
                        // if command.IsequallyShared is true, then amount should not be shown in form.
                        OwedAmount = owed is null || owed.IsEquallyShared ? null : owed.AmountOriginal       
                    });
            }

            return expenseViewModel;
        }
    }
}
