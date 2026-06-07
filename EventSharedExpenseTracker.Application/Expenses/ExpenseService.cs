using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.Commands;
using EventSharedExpenseTracker.Application.Expenses.Queries;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Domain.PaymentProcessing;
using Microsoft.Extensions.Logging;

namespace EventSharedExpenseTracker.Application.Expenses;

public class ExpenseService : IExpenseService
{
    private readonly ILogger<ExpenseService> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestContext _requestContext;
    private readonly IExchangeRateService _exchangeRateService;

    public ExpenseService(IUnitOfWork unitOfWork, IRequestContext requestContext, ILogger<ExpenseService> logger, IExchangeRateService exchangeRateService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<ServiceResult<TripExpensesQuery>> GetIndex(int tripId, string? sortOrder, string? searchString, bool creator, ExpenseCategory? categoryFilter)
    {
        // get and autorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForView(tripId);

        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<TripExpensesQuery>();

        var trip = tripResult.Value!;

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

        var query = new TripExpensesQuery
        {
            BaseCurrencyCode = trip.BaseCurrencyCode,
            Expenses = expenses.Select(e =>
            {
                var canEditExpense = AuthorisationRules.AuthorisedToEdit(e, userId);
                return ExpenseMapper.ToQuery(e, canEditExpense);
            }).ToList()
        };

        return query;
    }

    public async Task<ServiceResult<Expense>> Add(ExpenseCommand command, int tripId)
    {
        // get and autorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForView(tripId);

        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<Expense>();

        var trip = tripResult.Value!;

        var exchangeRate = await _exchangeRateService.GetRateAsync(command.CurrencyCode, trip.BaseCurrencyCode, command.Date);

        // process Expense's PaymentInputs to Payment entities. Validate their correctness.
        var paymentInputProcessingResult = ExpenseProcessor.ProcessForSaving(command.Payments, exchangeRate);
        if (!paymentInputProcessingResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentInputProcessingResult.Errors);
        var payments = paymentInputProcessingResult.Value;
        // map expenseCommand to Expense
        var context = new ExpenseCreationContext
        {
            TripId = tripId,
            UserId = userId,
            //TripBaseCurrencyCode = trip.BaseCurrencyCode,
            ExchangeRateToBase = exchangeRate
        };
        var expense = ExpenseMapper.FromCommand(command, context);
        // Expense attaches processed payments
        var setPaymentsResult = expense.SetPayments(payments);
        if (!setPaymentsResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(setPaymentsResult.Errors);

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} created for trip {TripId} by user {UserId}",
            expense.Id,
            expense.TripId,
            userId);

        return expense;
    }

    public async Task<ServiceResult<ExpenseQuery>> GetExpenseForm(int id)
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

    public async Task<ServiceResult<Expense>> Update(int id, ExpenseCommand command)
    {
        // get and autorise expense
        int userId = _requestContext.UserId;
        var expenseResult = await GetExpenseAuthorisedForEdit(id);

        if (!expenseResult.IsSuccess)
        {
            _logger.LogWarning("User {UserId} attempted update of expense {ExpenseId} without permission",
            userId, id);
            return expenseResult;
        }
        var existingExpense = expenseResult.Value!;

        // Get trip to get baseCurrency
        var trip = await _unitOfWork.Trips.GetByIdAsync(existingExpense.TripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        var exchangeRate = await GetExchangeRateForUpdateAsync(existingExpense, command, trip.BaseCurrencyCode);

        // process Expense's PaymentInputs to Payment entities. Validate their correctness.
        var paymentInputProcessingResult = ExpenseProcessor.ProcessForSaving(command.Payments, exchangeRate);
        if (!paymentInputProcessingResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentInputProcessingResult.Errors);
        var payments = paymentInputProcessingResult.Value;

        // apply changes from command to Expense
        ExpenseMapper.ApplyToExpense(existingExpense, command, exchangeRate);
        // Expense attaches processed payments
        var setPaymentsResult = existingExpense.SetPayments(payments);
        if (!setPaymentsResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(setPaymentsResult.Errors);

        //_unitOfWork.Expenses.Update(existingExpense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} updated by user {UserId}",
            existingExpense.Id,
            userId);

        return existingExpense;
    }

    public async Task<ServiceResult> Delete(int id)
    {
        // get and autorise expense
        int userId = _requestContext.UserId;
        var expenseResult = await GetExpenseAuthorisedForEdit(id);

        if (!expenseResult.IsSuccess)
        {
            _logger.LogWarning("User {UserId} attempted delete of expense {ExpenseId} without permission",
            userId, id);
            return expenseResult;
        }

        var expense = expenseResult.Value!;

        // delete Expense
        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Expense {ExpenseId} deleted by user {UserId}",
            id,
            userId);

        return ServiceResult.Ok();
    }

    private async Task<ServiceResult<Trip>> GetTripAuthorisedForView(int tripId)
    {
        var userId = _requestContext.UserId;
        var trip = await _unitOfWork.Trips.GetByIdAsync(tripId);
        if (trip == null)
            return AppErrors.NotFound<Trip>();

        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted unauthorized access of trip {TripId}",
            userId, trip.Id);
            return AppErrors.Forbidden<Trip>();
        }

        return trip;
    }

    private async Task<ServiceResult<Expense>> GetExpenseAuthorisedForEdit(int id)
    {
        var userId = _requestContext.UserId;
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id);
        if (expense == null)
            return AppErrors.NotFound<Expense>();

        if (!AuthorisationRules.AuthorisedToEdit(expense, userId))
            return AppErrors.Forbidden<Expense>();

        return expense;
    }

    private async Task<decimal> GetExchangeRateForUpdateAsync(
        Expense existingExpense,
        ExpenseCommand command,
        string baseCurrencyCode)
    {
        var currencyChanged = existingExpense.CurrencyCode != command.CurrencyCode;
        var dateChanged = existingExpense.Date != command.Date;

        if (!currencyChanged && !dateChanged)
            return existingExpense.ExchangeRateToBase;

        return await _exchangeRateService.GetRateAsync(
            command.CurrencyCode,
            baseCurrencyCode,
            command.Date);
    }
}
