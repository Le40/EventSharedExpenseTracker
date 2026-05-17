using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;

namespace EventSharedExpenseTracker.MvC.Mappers.Expenses
{
    public static class ExpenseVMMapper
    {
        public static ExpenseListItemViewModel FromQuery(ExpenseQuery query)
        {
            return new ExpenseListItemViewModel
            {
                Id = query.Id,
                Name = query.Name,
                Category = query.Category,
                Date = query.Date,
                //AmountSum = query.,
                CanEdit = query.CanEdit,
                Payments = query.Payments.Select(p => new PaymentDisplayViewModel
                {
                    ParticipantName = p.ParticipantName,
                    Amount = p.Amount,
                    IsOwed = p.IsOwed,
                    IsEquallyShared = p.IsEquallyShared
                }).ToList()
            };
        }
        public static ExpenseCommand ToCommand(ExpenseFormViewModel model, int tripId) //, int userId
        {
            var expenseCommand = new ExpenseCommand
            {
                Id = model.Id,
                TripId = tripId, // not needed, also in form cause its from route
                Name = model.Name,
                Date = model.Date,
                Category = model.Category,
                Description = model.Description
                //CreatorId = userId
            };

            foreach (var participant in model.Participants)
            { 
                if (participant.PaidAmount > 0) // validated in viewmodel 
                {
                    expenseCommand.Payments.Add(new PaymentCommand
                    {
                        ParticipantId = participant.ParticipantId,
                        Amount = participant.PaidAmount.Value,
                        IsOwed = false,
                        IsEquallyShared = false
                    });
                }

                if (participant.IsOwedSelected || participant.OwedAmount > 0) // validated in viewmodel
                {
                    expenseCommand.Payments.Add(new PaymentCommand
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

        public static ExpenseFormViewModel FromCommand(
            ExpenseCommand command,
            IEnumerable<TripParticipantQuery> tripParticipants)
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
                CanEdit = command.CanEdit,

                Name = command.Name,
                Date = command.Date,
                Category = command.Category,
                Description = command.Description
            };

            foreach (var participant in tripParticipants)
            {
                paidByParticipant.TryGetValue(participant.Id, out var paid);
                owedByParticipant.TryGetValue(participant.Id, out var owed);

                expenseViewModel.Participants.Add(
                    new ExpenseFormParticipantViewModel
                    {
                        ParticipantId = participant.Id,
                        ParticipantName = participant.UserName,

                        PaidPaymentId = paid?.Id,
                        PaidAmount = paid?.Amount,

                        OwedPaymentId = owed?.Id,
                        // every owed payment is selected in form.
                        IsOwedSelected = owed != null,
                        // if command.IsequallyShared is true, then amount should not be shown in form.
                        OwedAmount = owed is null || owed.IsEquallyShared ? null : owed.Amount       
                    });
            }

            return expenseViewModel;
        }
    }
}
