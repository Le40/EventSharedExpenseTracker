using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Application.Services.Utility;

namespace EventSharedExpenseTracker.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationServ _authorizationService;
    private readonly IRequestContext _requestContext;

    public ExpenseService(IUnitOfWork unitOfWork, IAuthorisationServ authorisationService, IRequestContext requestContext)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
    }

    public async Task<ServiceResult<List<Expense>>> Index(int tripId, string sortOrder, string searchString, string creator, string categoryFilter)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return new ServiceResult<List<Expense>>("Needed resource not found.", 404);

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return new ServiceResult<List<Expense>>("Insufficient permissions.", 403);

        var orderBy = ExpenseHelper.GetOrderByExpression(sortOrder);

        var listOfFilters = new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
        {
            //if (searchString != null)
            ExpenseHelper.Search(searchString),

            //if (categoryFilter != null)
            ExpenseHelper.CategoryFilter(categoryFilter)
        };

        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        return new ServiceResult<List<Expense>> (expenses, 200);
    }


    public async Task<ServiceResult<Expense>> Create(int tripId)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return new ServiceResult<Expense>("Insufficient permissions.", 403);

        var orderedParticipants = trip.Participants
            .OrderBy(p => p.UserId == userId ? 0 : 1)
            .ThenBy(p => p.UserName)
            .ToList();

        var expense = new Expense
        {
            CreatorId = userId,
            TripId = trip.Id
        };

        foreach (var participant in orderedParticipants)
        {
            // for each participant create two payments for paid and one for owed payments
            foreach (bool isOwed in new[] { false, true })
            {
                var payment = new Payment
                {
                    UserId = participant.UserId,
                    ParticipantId = participant.Id,
                    Participant = participant,
                    IsOwed = isOwed,
                    //UserName = participant.UserName
                };
                expense.Payments.Add(payment);
            }
        }

        return new ServiceResult<Expense>(expense, 200);
    }

    public async Task<ServiceResult<Expense>> Add(Expense expense, int tripId)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return new ServiceResult<Expense>("Insufficient permissions.", 403);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Expense>("Success", 200);
    }

    public async Task<ServiceResult<Expense>> Get(int id, int tripId)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        if (!_authorizationService.AuthorisedToEdit(expense, userId))
            return new ServiceResult<Expense>("Insufficient permissions.", 403);

        if (expense.TripId != tripId)
            return new ServiceResult<Expense>("Bad Request", 400);

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        expense.Payments = expense.Payments
            .GroupBy(p => p.ParticipantId) // Group payments by user ID
            .OrderByDescending(group => group.Max(p => p?.Ammount ?? 0)) // Order the groups based on the highest amount within each group
            .SelectMany(group => group.OrderByDescending(p => p?.Ammount ?? 0)) // Take the two highest payments for each user
            .ToList();

        return new ServiceResult<Expense>(expense, 200);
    }

    public async Task<ServiceResult<Expense>> Update(Expense expense)
    {
        int userId = _requestContext.UserId;

        if(!_authorizationService.AuthorisedToEdit(expense, userId))
            return new ServiceResult<Expense>("Insufficient permissions.", 403);

        var trip = await _unitOfWork.Trips.GetByIdAsync(expense.TripId);
        if (trip == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Expense>("Success", 200);
    }


    public async Task<ServiceResult<Expense>> Delete (int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return new ServiceResult<Expense>("Needed resource not found.", 404);

        if (!_authorizationService.AuthorisedToEdit(expense, userId))
            return new ServiceResult<Expense>("Insufficient permissions.", 403);

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        return new ServiceResult<Expense>("Success", 200);
    }


    public async Task<Expense> LoadParticipants(Expense expense)
    {
        var trip = await _unitOfWork.Trips.GetByIdAsync(expense.TripId);
        foreach (var payment in expense.Payments)
        {
            payment.Participant = trip?.Participants.FirstOrDefault(p => p.Id == payment.ParticipantId);
        }
        return expense;
    }
}
