using EventSharedExpenseTracker.Application.Validation;
using EventSharedExpenseTracker.Domain.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                foreach (var error in result.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName ?? "", error.Message);
                }
            }
        }

        else if (context.ActionArguments.TryGetValue("trip", out var tripObj) && tripObj is Trip trip)
        {
            var result = _validationService.ValidateTrip(trip);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    context.ModelState.AddModelError(error.PropertyName ?? "", error.Message);
                }
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
