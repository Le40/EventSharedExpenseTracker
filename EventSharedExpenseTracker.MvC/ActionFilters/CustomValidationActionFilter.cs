using Microsoft.AspNetCore.Mvc.Filters;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Validation;

namespace EventSharedExpenseTracker.MvC.ActionFilters;

public class CustomValidationActionFilter : IActionFilter
{
    private readonly IValidationService _validationService;

    public CustomValidationActionFilter(IValidationService validationService)
    {
        _validationService = validationService;
    }
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("expense", out var expenseObj) && expenseObj is Expense expense)
        {
            var result = _validationService.ValidateExpense(expense);
            if (!result.IsValid)
            {
                context.ModelState.AddModelError("", result.ErrorMessage ?? string.Empty);
            }
        }
        else if (context.ActionArguments.TryGetValue("trip", out var tripObj) && tripObj is Trip trip)
        {
            var result = _validationService.ValidateTrip(trip);
            if (!result.IsValid)
            {
                context.ModelState.AddModelError("", result.ErrorMessage ?? string.Empty);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
