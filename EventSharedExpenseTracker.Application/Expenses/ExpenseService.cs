using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Validation;
using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EventSharedExpenseTracker.Application.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly ILogger<ExpenseService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationService _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IValidationService _validationService;

    public ExpenseService(IUnitOfWork unitOfWork, IAuthorisationService authorisationService, IRequestContext requestContext, IValidationService validationService, ILogger<ExpenseService> logger)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
        _validationService = validationService;
    }

    public async Task<Result<List<ExpenseQuery>>> Index(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId}",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        var options = new ExpenseQueryOptions
        {
            UserId = userId,
            SearchString = searchString,
            SortBy = sortOrder,
            CreatedByMe = creator,
            Category = categoryFilter
        };

        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId, options);

        var items = expenses.Select(e => {
            var canEditExpense = _authorizationService.AuthorisedToEdit(e, userId);
            return ExpenseMapper.ToQuery(e, canEditExpense);
            }).ToList();

        return items;
    }

    public async Task<Result<Expense>> Add(ExpenseCommand command, int tripId)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToView(trip, userId)) 
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId} to CREATE new expense.",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //var expense = processedCommand.Adapt<Expense>();
        var expense = ExpenseMapper.ToExpense(processedCommand, tripId, userId);

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

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        var canEdit = _authorizationService.AuthorisedToEdit(expense, userId);

        //var command = expense.Adapt<ExpenseCommand>();
        var query = ExpenseMapper.ToQuery(expense, canEdit);

        return query;
    }

    public async Task<Result<Expense>> Update(int id, ExpenseCommand command)
    {
        int userId = _requestContext.UserId;

        var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (existingExpense == null)
            return AppErrors.NotFound<Expense>();

        if (!_authorizationService.AuthorisedToEdit(existingExpense, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized UPDATE of expense {ExpenseId}",
            userId, existingExpense.Id);
            return AppErrors.Forbidden<Expense>();
        }

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //processedCommand.Adapt(existingExpense);
        ExpenseMapper.ApplyToExpense(existingExpense, processedCommand);

        //_unitOfWork.Expenses.Update(existingExpense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} updated by user {UserId}",
            existingExpense.Id,
            userId);

        return existingExpense;
    }

    public async Task<Result> Delete(int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        if (!_authorizationService.AuthorisedToEdit(expense, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized DELETE of expense {ExpenseId}",
            userId, id);
            return AppErrors.Forbidden<Expense>();
        }

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} deleted by user {UserId}",
            id,
            userId);

        return Result.Ok();
    }
}
