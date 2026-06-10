using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Application.Trips;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.MvC.Common;
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

        public async Task<ServiceResult<ExpenseFormViewModel>> BuildCreateAsync(int tripId)
        {
            var tripResult = await _tripService.GetParticipants(tripId);

            if (!tripResult.IsSuccess)
                return ServiceResult<ExpenseFormViewModel>.Fail(tripResult.Errors);

            var userId = _requestContext.UserId;
            string userName = _requestContext.UserName;

            var orderedParticipants = tripResult.Value!.Participants
                .OrderBy(p => p.DisplayName == userName ? 0 : 1)
                .ThenBy(p => p.DisplayName)
                .ToList();

            var model = new ExpenseFormViewModel
            {
                //CreatorId = userId,
                FormId = "expense-createForm",
                CanUserEdit = true,
                TripId = tripId,
                CurrencyCode = tripResult.Value.BaseCurrencyCode,
                CurrencyOptions = CurrencySelectList.Get(),
                //CategoryOptions = ExpenseCategorySelectList.Get(),

                Participants = orderedParticipants.Select(p =>
                    new ExpenseFormParticipantViewModel
                    {
                        ParticipantId = p.Id,
                        ParticipantName = p.DisplayName,
                        PaidAmount = null,
                        IsOwedSelected = false,
                        OwedAmount = null
                    }).ToList()
            };

            return ServiceResult<ExpenseFormViewModel>.Ok(model);
        }

        public async Task<ServiceResult<ExpenseFormViewModel>> BuildEditAsync(int expenseId)
        {
            var queryResult = await _expenseService.GetExpenseForm(expenseId);
            if (!queryResult.IsSuccess)
                return ServiceResult<ExpenseFormViewModel>.Fail(queryResult.Errors);
                
            var query = queryResult.Value!;

            var tripResult = await _tripService.GetParticipants(query.TripId);

            if (!tripResult.IsSuccess)
                return ServiceResult<ExpenseFormViewModel>.Fail(tripResult.Errors);

            //var userId = _requestContext.UserId;

            var model = ExpenseVMMapper.FromQuery(query, tripResult.Value!.Participants);

            return ServiceResult<ExpenseFormViewModel>.Ok(model);
        }
    }
}
