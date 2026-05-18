using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.Application.Services.Utility;
using EventSharedExpenseTracker.Application.Validation;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Dtos.Mappers;
using Mapster;

namespace EventSharedExpenseTracker.Application.Services;

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
            return Result<List<ExpenseQuery>>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return Result<List<ExpenseQuery>>.Fail(AppErrors.Forbidden<Trip>());

        var orderBy = ExpenseFilters.GetOrderByExpression(sortOrder);

        var listOfFilters = new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
        {
            ExpenseFilters.Search(searchString!),
            ExpenseFilters.CategoryFilter(categoryFilter!),
            ExpenseFilters.CreatorFilter(creator, userId)
        };

        // Done like this, so only necessary amount of row is queried from db.
        // And to not have dependency on EF core in application.
        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        var items = expenses.Select(e => {
            var canEditExpense = _authorizationService.AuthorisedToEdit(e, userId);
            return ExpenseMapper.ToQuery(e, canEditExpense);
            }).ToList();

        return Result<List<ExpenseQuery>>.Ok(items);
    }

    public async Task<Result<Expense>> Add(ExpenseCommand command, int tripId)
    {
        int userId = _requestContext.UserId;

        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return Result<Expense>.Fail(AppErrors.NotFound<Trip>());

        if (!_authorizationService.AuthorisedToView(trip, userId))
            return Result<Expense>.Fail(AppErrors.Forbidden<Trip>());

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //var expense = processedCommand.Adapt<Expense>();
        var expense = ExpenseMapper.ToExpense(processedCommand, tripId, userId);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

        return Result<Expense>.Ok(expense);
    }

    public async Task<Result<ExpenseQuery>> GetExpenseForm(int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return Result<ExpenseQuery>.Fail(AppErrors.NotFound<Expense>());

        var canEdit = _authorizationService.AuthorisedToEdit(expense, userId);

        //var command = expense.Adapt<ExpenseCommand>();
        var query = ExpenseMapper.ToQuery(expense, canEdit);

        return Result<ExpenseQuery>.Ok(query);
    }

    public async Task<Result<Expense>> Update(int id, ExpenseCommand command)
    {
        int userId = _requestContext.UserId;

        var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (existingExpense == null)
            return Result<Expense>.Fail(AppErrors.NotFound<Expense>());

        if (!_authorizationService.AuthorisedToEdit(existingExpense, userId))
            return Result<Expense>.Fail(AppErrors.Forbidden<Expense>());

        var validationResult = _validationService.ProcessForSaving(command);

        if (!validationResult.IsSuccess)
            return Result<Expense>.Fail(validationResult.Errors);

        var processedCommand = validationResult.Value!;

        //processedCommand.Adapt(existingExpense);
        ExpenseMapper.ApplyToExpense(existingExpense, processedCommand);

        //_unitOfWork.Expenses.Update(existingExpense);
        await _unitOfWork.CompleteAsync();

        return Result<Expense>.Ok(existingExpense);
    }

    public async Task<Result> Delete(int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return Result.Fail(AppErrors.NotFound<Expense>());

        if (!_authorizationService.AuthorisedToEdit(expense, userId))
            return Result.Fail(AppErrors.Forbidden<Expense>());

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        return Result.Success();
    }
}
