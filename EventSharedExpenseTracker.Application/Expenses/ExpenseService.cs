using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Domain.Enums;
using Microsoft.Extensions.Logging;
using Mapster;
using EventSharedExpenseTracker.Domain.PaymentProcessing;

namespace EventSharedExpenseTracker.Application.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly ILogger<ExpenseService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestContext _requestContext;

    public ExpenseService(IUnitOfWork unitOfWork, IRequestContext requestContext, ILogger<ExpenseService> logger)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
    }

    public async Task<Result<List<ExpenseQuery>>> Index(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter)
    {
        int userId = _requestContext.UserId;

        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise Trip
        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId}",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        // options for query
        var options = new ExpenseQueryOptions
        {
            UserId = userId,
            SearchString = searchString,
            SortBy = sortOrder,
            CreatedByMe = creator,
            Category = categoryFilter
        };

        // get Expenses
        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId, options);

        // map to query/response + authorise each Expense
        var items = expenses.Select(e => {
            var canEditExpense = AuthorisationRules.AuthorisedToEdit(e, userId);
            return ExpenseMapper.ToQuery(e, canEditExpense);
            }).ToList();

        return items;
    }

    public async Task<Result<Expense>> Add(ExpenseCommand command, int tripId)
    {
        int userId = _requestContext.UserId;

        // get Trip
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        // authorise Trip
        if (!AuthorisationRules.AuthorisedToView(trip, userId)) 
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId} to CREATE new expense.",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        // process Expense's PaymentInputs to Payment entities. Validate their correctness.
        var paymentInputProcessingResult = ExpenseProcessor.ProcessForSaving(command.Payments);
        if (!paymentInputProcessingResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentInputProcessingResult.Errors);

        // map expenseCommand to Expense
        var expense = ExpenseMapper.ToExpense(command, tripId, userId);
        // Expense attaches processed payments
        expense.SetPayments(paymentInputProcessingResult.Value);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} created for trip {TripId} by user {UserId}",
            expense.Id,
            expense.TripId,
            userId);

        return expense;
    }

    public async Task<Result<ExpenseQuery>> GetExpenseForm(int id)
    {
        int userId = _requestContext.UserId;

        // get Expense
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        // authorise Expense - if canUserEdit is false, user can still view the form, just not post it
        var canUserEdit = AuthorisationRules.AuthorisedToEdit(expense, userId);

        // map Expense to expense query/request
        var query = ExpenseMapper.ToQuery(expense, canUserEdit);

        return query;
    }

    public async Task<Result<Expense>> Update(int id, ExpenseCommand command)
    {
        int userId = _requestContext.UserId;

        // get Expense
        var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (existingExpense == null)
            return AppErrors.NotFound<Expense>();

        // authorise Expense
        if (!AuthorisationRules.AuthorisedToEdit(existingExpense, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized UPDATE of expense {ExpenseId}",
            userId, existingExpense.Id);
            return AppErrors.Forbidden<Expense>();
        }

        // process Expense's PaymentInputs to Payment entities. Validate their correctness.
        var paymentInputProcessingResult = ExpenseProcessor.ProcessForSaving(command.Payments);
        if (!paymentInputProcessingResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentInputProcessingResult.Errors);

        // apply changes from command to Expense
        ExpenseMapper.ApplyToExpense(existingExpense, command);
        // Expense attaches processed payments
        existingExpense.SetPayments(paymentInputProcessingResult.Value);

        //_unitOfWork.Expenses.Update(existingExpense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} updated by user {UserId}",
            existingExpense.Id,
            userId);

        return existingExpense;
    }

    public async Task<ServiceResult> Delete(int id)
    {
        int userId = _requestContext.UserId;

        // get Expense
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        // authorise Expense
        if (!AuthorisationRules.AuthorisedToEdit(expense, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized DELETE of expense {ExpenseId}",
            userId, id);
            return AppErrors.Forbidden<Expense>();
        }

        // delete Expense
        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} deleted by user {UserId}",
            id,
            userId);

        return ServiceResult.Ok();
    }
}
