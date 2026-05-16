using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Domain.Models;

namespace EventSharedExpenseTracker.MvC.ViewModels.Expenses
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

        public async Task<Result<ExpenseViewModel>> BuildCreateAsync(int tripId)
        {
            var tripResult = await _tripService.Details(tripId);

            if (!tripResult.IsSuccess)
                return Result<ExpenseViewModel>.Fail(tripResult.Errors);

            var userId = _requestContext.UserId;

            var orderedParticipants = tripResult.Value!.Participants
            //var orderedParticipants = tripResult.Data!.Participants
                .OrderBy(p => p.UserId == userId ? 0 : 1)
                .ThenBy(p => p.UserName)
                .ToList();

            var model = new ExpenseViewModel
            {
                //CreatorId = userId,
                CanEdit = true,
                TripId = tripId,
                Categories = Expense.Categories,

                Participants = orderedParticipants.Select(p =>
                    new ExpenseParticipantViewModel
                    {
                        ParticipantId = p.Id,
                        ParticipantName = p.UserName,
                        PaidAmount = null,
                        IsOwedSelected = false,
                        OwedAmount = null
                    }).ToList()
            };

            return Result<ExpenseViewModel>.Ok(model);
        }

        public async Task<Result<ExpenseViewModel>> BuildEditAsync(int expenseId)
        {
            var commandResult = await _expenseService.GetForUpdate(expenseId);
            if (!commandResult.IsSuccess)
                return Result<ExpenseViewModel>.Fail(commandResult.Errors);
                
            var command = commandResult.Value!;

            var tripResult = await _tripService.Details(command.TripId);

            if (!tripResult.IsSuccess)
                return Result<ExpenseViewModel>.Fail(tripResult.Errors);

            var userId = _requestContext.UserId;

            var model = ExpenseFormMapper.FromCommand(command, tripResult.Value!.Participants);

            return Result<ExpenseViewModel>.Ok(model);
        }
    }
}
