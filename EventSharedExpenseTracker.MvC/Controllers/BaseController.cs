using EventSharedExpenseTracker.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IActionResult HandleServiceErrors(IEnumerable<AppError> errors)
        {
            if (errors.Any(e => e.Type == AppErrorType.NotFound))
                return NotFound();

            if (errors.Any(e => e.Type == AppErrorType.Forbidden))
                return Forbid();

            if (TryAddValidationErrorsToModelState(errors.ToList()))
                return BadRequest(ModelState);

            return StatusCode(500);
        }

        protected bool TryAddValidationErrorsToModelState(IEnumerable<AppError> errors)
        {
            var validationErrors = errors
                .Where(e => e.Type == AppErrorType.Validation)
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
