using EventSharedExpenseTracker.Application.Expenses.Commands;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.PaymentProcessing;
using EventSharedExpenseTracker.Domain.ValueObjects;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;

namespace EventSharedExpenseTracker.MvC.Mappers.Expenses
{
    public static class ExpenseVMMapper
    {
        public static ExpenseListItemViewModel FromQuery(ExpenseQuery query, string tripCurrencyCode)
        {
            var paidPayments = query.Payments.Where(p => !p.IsOwed).ToList();
            var owedPayments = query.Payments.Where(p => p.IsOwed).ToList();


            return new ExpenseListItemViewModel
            {
                Id = query.Id,
                Name = query.Name,
                Category = query.Category,
                Date = query.Date,
                CanUserEdit = query.CanUserEdit,
                PaidPayments = paidPayments,
                OwedPayments = owedPayments,
                TotalPaidBase = new Money(paidPayments.Sum(p => p.AmountBase), tripCurrencyCode),
                TotalPaidOriginal = new Money(paidPayments.Sum(p => p.AmountOriginal), query.CurrencyCode)
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
                    expenseCommand.Payments.Add(new PaymentDraft
                    {
                        ParticipantId = participant.ParticipantId,
                        UserEnteredAmount = participant.PaidAmount.Value,
                        IsOwed = false,
                        IsEquallyShared = false
                    });
                }

                if (participant.IsOwedSelected || participant.OwedAmount > 0) // validated in viewmodel
                {
                    expenseCommand.Payments.Add(new PaymentDraft
                    {
                        ParticipantId = participant.ParticipantId,
                        UserEnteredAmount = participant.OwedAmount,
                        IsOwed = true,
                        IsEquallyShared = participant.IsOwedSelected
                    });
                }
            }

            return expenseCommand;
        }

        public static ExpenseFormViewModel FromQuery(
            ExpenseQuery query,
            IEnumerable<TripParticipantQuery> tripParticipants)
        {
            var paidByParticipant = query.Payments
                .Where(p => !p.IsOwed)
                .ToDictionary(p => p.ParticipantId);

            var owedByParticipant = query.Payments
                .Where(p => p.IsOwed)
                .ToDictionary(p => p.ParticipantId);

            var expenseViewModel = new ExpenseFormViewModel
            {
                FormId = $"expense-edit-{query.Id}",
                Id = query.Id,
                TripId = query.TripId,
                CanUserEdit = query.CanUserEdit,

                Name = query.Name,
                Date = query.Date,
                Category = query.Category,
                //CategoryOptions = ExpenseCategorySelectList.Get(),
                Description = query.Description,
                CurrencyCode = query.CurrencyCode,
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
