using EventSharedExpenseTracker.Application.Common.Authorisation;
using EventSharedExpenseTracker.Application.Common.Constants;
using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses.Commands;
using EventSharedExpenseTracker.Application.Expenses.DTOs;
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
    private readonly IImageService _imageService;
    private readonly IExpenseAiService _aiService;

    public ExpenseService(IUnitOfWork unitOfWork, IRequestContext requestContext, ILogger<ExpenseService> logger, IExchangeRateService exchangeRateService, IImageService imageService, IExpenseAiService aiService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _requestContext = requestContext;
        _exchangeRateService = exchangeRateService;
        _imageService = imageService;
        _aiService = aiService;
    }

    public async Task<ServiceResult<TripExpensesQuery>> GetIndex(int tripId, string? searchString)
    {
        // get and autorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForView(tripId);

        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<TripExpensesQuery>();

        var trip = tripResult.Value!;

        // options for query
        var options = new ExpenseFilterOptions
        {
            UserId = userId,
            SearchString = searchString,
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
        // get and authorise trip
        int userId = _requestContext.UserId;
        var tripResult = await GetTripAuthorisedForView(tripId);

        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<Expense>();

        var trip = tripResult.Value!;

        // prevention of double sync
        var offlineRetryResult = await CheckOfflineRetryAsync(command.OfflineClientId, tripId);

        if (offlineRetryResult is not null)
            return offlineRetryResult;

        // Build validated payment entities from input.
        var exchangeRate = await _exchangeRateService.GetRateAsync(command.CurrencyCode, trip.BaseCurrencyCode, command.Date);
        var participantIds = trip.Participants.Select(p => p.Id).ToHashSet();

        var paymentsResult = ExpenseProcessor.BuildPayments(command.Payments, participantIds, exchangeRate);
        if (!paymentsResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentsResult.Errors);

        var expenseResult = Expense.Create(
            name: command.Name,
            date: command.Date,
            category: command.Category,
            description: command.Description,
            currencyCode: command.CurrencyCode,
            offlineClientId: command.OfflineClientId,
            tripId: tripId,
            userId: userId,
            exchangeRateToBase: exchangeRate,
            payments: paymentsResult.Value!);

        if (!expenseResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(expenseResult.Errors);

        var expense = expenseResult.Value!;

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

        // get Expense and authorise
        var expenseResult = await GetExpenseAuthorisedForEdit(id);

        if (!expenseResult.IsSuccess)
        {
            _logger.LogWarning("User {UserId} attempted edit expense {ExpenseId} without permission",
            userId, id);
            return expenseResult.ToFailure<ExpenseQuery>();
        }

        var expense = expenseResult.Value!;

        var query = ExpenseMapper.ToQuery(expense, canUserEdit: true);

        return query;
    }

    public async Task<ServiceResult<Expense>> Update(int id, ExpenseCommand command)
    {
        // get and authorise expense
        int userId = _requestContext.UserId;

        var expenseResult = await GetExpenseAuthorisedForEdit(id);
        if (!expenseResult.IsSuccess)
            return expenseResult;

        var existingExpense = expenseResult.Value!;

        // Get trip to get baseCurrency
        var tripResult = await GetTripAuthorisedForView(existingExpense.TripId);
        if (!tripResult.IsSuccess)
            return tripResult.ToFailure<Expense>();

       var trip = tripResult.Value!;

        // Build validated payment entities from input.
        var exchangeRate = await GetExchangeRateForUpdateAsync(existingExpense, command, trip.BaseCurrencyCode);
        var participantIds = trip.Participants.Select(p => p.Id).ToHashSet();

        var paymentsResult = ExpenseProcessor.BuildPayments(command.Payments, participantIds, exchangeRate);
        if (!paymentsResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(paymentsResult.Errors);

        var updateResult = existingExpense.Update(
            name: command.Name,
            date: command.Date,
            category: command.Category,
            description: command.Description,
            currencyCode: command.CurrencyCode,
            exchangeRateToBase: exchangeRate,
            payments: paymentsResult.Value!);

        if (!updateResult.IsSuccess)
            return DomainErrorMapper.ToAppErrors(updateResult.Errors);

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

    private async Task<ServiceResult<Expense>> GetExpenseAuthorisedForView(
    int expenseId)
    {
        var userId = _requestContext.UserId;
        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);

        if (expense is null)
            return AppErrors.NotFound<Expense>();

        var trip = await _unitOfWork.Trips.GetByIdAsync(expense.TripId);

        if (trip is null)
            return AppErrors.NotFound<Expense>();

        if (!AuthorisationRules.AuthorisedToView(trip, userId))
        {
            _logger.LogWarning("User {UserId} attempted view expense {ExpenseId} without permission",userId, expenseId);
            return AppErrors.Forbidden<Expense>();
        }

        return expense;
    }

    private async Task<ServiceResult<Expense>> GetExpenseAuthorisedForEdit(int id)
    {
        var userId = _requestContext.UserId;
        var expenseResult = await GetExpenseAuthorisedForView(id);

        if (!expenseResult.IsSuccess)
            return expenseResult;

        var expense = expenseResult.Value!;

        if (!AuthorisationRules.AuthorisedToEdit(expense, userId))
        {
            _logger.LogWarning("User {UserId} attempted modify expense {ExpenseId} without permission", userId, id);
            return AppErrors.Forbidden<Expense>();
        }

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

    public async Task<ServiceResult<ReceiptParseResult>> ExtractReceiptDataAsync(Stream imageStream)
    {
        // check for too big files
        if (imageStream is null || !imageStream.CanRead)
        {
            return AppErrors.Validation<ReceiptParseResult>(
                "No valid image was provided.");
        }

        if (imageStream.CanSeek)
        {
            if (imageStream.Length == 0)
            {
                return AppErrors.Validation<ReceiptParseResult>(
                    "The image is empty.");
            }

            if (imageStream.Length > ImageUploadLimits.MaxImageBytes)
            {
                return AppErrors.Validation<ReceiptParseResult>(
                    $"Image cannot be larger than " +
                    $"{ImageUploadLimits.MaxImageBytes / 1024 / 1024} MB.");
            }

            imageStream.Position = 0;
        }


        var imageBytes = await _imageService.ResizeAndCompressAsync(
            imageStream,
            maxWidth: 1920,
            maxHeight: 1920,
            quality: 90);

        var parsedReceipt = await _aiService.ParseReceiptAsync(imageBytes, "image/jpeg");
        if (parsedReceipt.Confidence < 0.5m || parsedReceipt.TotalAmount is null)
            return AppErrors.Notification<ReceiptParseResult>("The receipt could not be parsed reliably. Please try another photo.");

        return parsedReceipt;
    }

    public async Task<ServiceResult<ExpenseCategory>> SuggestCategoryAsync(string expenseName)
    {
        var categories = Enum.GetNames<ExpenseCategory>();

        var suggestion =  await _aiService.SuggestCategoryAsync(
            expenseName,
            categories);

        return Enum.Parse<ExpenseCategory>(suggestion.SuggestedCategory);

    }

    private async Task<ServiceResult<Expense>?> CheckOfflineRetryAsync(Guid? offlineClientId, int tripId)
    {
        if (!offlineClientId.HasValue)
            return null;

        var existingExpense = await _unitOfWork.Expenses
            .GetByOfflineIdAsync(offlineClientId.Value);

        if (existingExpense is null)
            return null;

        if (existingExpense.TripId != tripId)
            return AppErrors.Conflict<Expense>();

        return existingExpense;
    }
}
