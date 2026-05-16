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

            if (errors.Any(e => e.Type == ErrorType.Validation))
            {
                foreach (var error in errors.Where(e => e.Type == ErrorType.Validation))
                {
                    ModelState.AddModelError(
                        error.PropertyName ?? "",
                        error.Message);
                }

                return BadRequest(ModelState);
            }

            return StatusCode(500);
        }

        protected void AddErrorsToModelState(IEnumerable<AppError> errors)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(
                    error.PropertyName ?? "",
                    error.Message);
            }
        }
    }
}
