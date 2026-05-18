using EventSharedExpenseTracker.Domain.Models;
using Mapster;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Validation;
using EventSharedExpenseTracker.Application.Common.Authorisation;

namespace EventSharedExpenseTracker.Application.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthorisationService _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IValidationService _validationService;

    public ExpenseService(IUnitOfWork unitOfWork, IAuthorisationService authorisationService, IRequestContext requestContext, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
        _validationService = validationService;
    }

    public async Task<Result<List<ExpenseQuery>>> Index(int tripId, string? sortOrder, string? searchString, bool creator, string? categoryFilter)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return AppErrors.Forbidden<Trip>();

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
            return AppErrors.Forbidden<Trip>();

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //var expense = processedCommand.Adapt<Expense>();
        var expense = ExpenseMapper.ToExpense(processedCommand, tripId, userId);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

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
            return AppErrors.Forbidden<Expense>();

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //processedCommand.Adapt(existingExpense);
        ExpenseMapper.ApplyToExpense(existingExpense, processedCommand);

        //_unitOfWork.Expenses.Update(existingExpense);
        await _unitOfWork.CompleteAsync();

        return existingExpense;
    }

    public async Task<Result> Delete(int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        if (!_authorizationService.AuthorisedToEdit(expense, userId))
            return AppErrors.Forbidden<Expense>();

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        return Result.Ok();
    }
}
