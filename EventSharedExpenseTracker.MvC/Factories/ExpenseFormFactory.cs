using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;

namespace EventSharedExpenseTracker.MvC.Factories
{
    public class ExpenseFormFactory
    {
        private readonly ITripService _tripService;
        private readonly IRequestContext _requestContext;
        private readonly IExpenseService _expenseService;

        public ExpenseFormFactory(
            ITripService tripService,
            IRequestContext requestContext,
            IExpenseService expenseService)
        {
            _tripService = tripService;
            _requestContext = requestContext;
            _expenseService = expenseService;
        }

        public async Task<Result<ExpenseFormViewModel>> BuildCreateAsync(int tripId)
        {
            var tripResult = await _tripService.Details(tripId);

            if (!tripResult.IsSuccess)
                return Result<ExpenseFormViewModel>.Fail(tripResult.Errors);

            var userId = _requestContext.UserId;
            string userName = _requestContext.UserName;

            var orderedParticipants = tripResult.Value!.Participants
            //var orderedParticipants = tripResult.Data!.Participants
                .OrderBy(p => p.UserName == userName ? 0 : 1)
                .ThenBy(p => p.UserName)
                .ToList();

            var model = new ExpenseFormViewModel
            {
                //CreatorId = userId,
                CanUserEdit = true,
                TripId = tripId,
                Categories = Expense.Categories,

                Participants = orderedParticipants.Select(p =>
                    new ExpenseFormParticipantViewModel
                    {
                        ParticipantId = p.Id,
                        ParticipantName = p.UserName,
                        PaidAmount = null,
                        IsOwedSelected = false,
                        OwedAmount = null
                    }).ToList()
            };

            return Result<ExpenseFormViewModel>.Ok(model);
        }

        public async Task<Result<ExpenseFormViewModel>> BuildEditAsync(int expenseId)
        {
            var commandResult = await _expenseService.GetForUpdate(expenseId);
            if (!commandResult.IsSuccess)
                return Result<ExpenseFormViewModel>.Fail(commandResult.Errors);
                
            var command = commandResult.Value!;

            var tripResult = await _tripService.Details(command.TripId);

            if (!tripResult.IsSuccess)
                return Result<ExpenseFormViewModel>.Fail(tripResult.Errors);

            var userId = _requestContext.UserId;

            var model = ExpenseVMMapper.FromCommand(command, tripResult.Value!.Participants);

            return Result<ExpenseFormViewModel>.Ok(model);
        }
    }
}
