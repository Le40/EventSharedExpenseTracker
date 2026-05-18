using EventSharedExpenseTracker.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IActionResult HandleServiceErrors(IEnumerable<AppError> errors)
        {
            if (errors.Any(e => e.Type == ErrorType.NotFound))
                return NotFound();

            if (errors.Any(e => e.Type == ErrorType.Forbidden))
                return Forbid();

            if (TryAddValidationErrorsToModelState(errors.ToList()))
                return BadRequest(ModelState);

            return StatusCode(500);
        }

        protected bool TryAddValidationErrorsToModelState(IEnumerable<AppError> errors)
        {
            var validationErrors = errors
                .Where(e => e.Type == ErrorType.Validation)
                .ToList();

            if (!validationErrors.Any())
                return false;

            foreach (var error in validationErrors)
            {
                ModelState.AddModelError(
                    error.PropertyName ?? "",
                    error.Message);
            }
            return true;
        }
    }
}
