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
    private readonly IAuthorisationServ _authorizationService;
    private readonly IRequestContext _requestContext;
    private readonly IValidationService _validationService;

    public ExpenseService(IUnitOfWork unitOfWork, IAuthorisationServ authorisationService, IRequestContext requestContext, IValidationService validationService)
    {
        _unitOfWork = unitOfWork;
        _authorizationService = authorisationService;
        _requestContext = requestContext;
        _validationService = validationService;
    }

    /*public async Task<ServiceResult<List<Expense>>> Index(int tripId, string sortOrder, string searchString, bool creator, string categoryFilter)
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
            ExpenseHelper.CategoryFilter(categoryFilter),

            ExpenseHelper.CreatorFilter(creator, userId)
        };

        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        return new ServiceResult<List<Expense>> (expenses, 200);
    }*/
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

        var expenses = await _unitOfWork.Expenses.GetAllFromTripAsync(tripId,
            orderBy: orderBy,
            filters: listOfFilters.ToArray());

        var items = expenses.Select(e => ExpenseMapper.MapToQuery(e, userId)).ToList();

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
        var expense = ExpenseMapper.MapToExpense(processedCommand, tripId, userId);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

        return Result<Expense>.Ok(expense);
    }

    public async Task<Result<ExpenseCommand>> GetForUpdate(int id)
    {
        int userId = _requestContext.UserId;

        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return Result<ExpenseCommand>.Fail(AppErrors.NotFound<Expense>());

        var canEdit = _authorizationService.AuthorisedToEdit(expense, userId);

        //var command = expense.Adapt<ExpenseCommand>();
        var command = ExpenseMapper.MapToCommand(expense, canEdit);

        return Result<ExpenseCommand>.Ok(command);
    }

    public async Task<Result<Expense>> Update(ExpenseCommand command)
    {
        int userId = _requestContext.UserId;

        var existingExpense = await _unitOfWork.Expenses.GetByIdAsync(command.Id);
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
