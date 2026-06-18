using EventSharedExpenseTracker.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IActionResult HandleServiceErrors(IEnumerable<AppError> errors)
        {
            var toastError = errors.FirstOrDefault(e => e.Type == AppErrorType.Notification);
            if (toastError is not null)
            {
                TriggerToast(toastError.Message, "warning");
                return NoContent();
            }

            if (errors.Any(e => e.Type == AppErrorType.NotFound))
                return NotFound();

            if (errors.Any(e => e.Type == AppErrorType.Forbidden))
                return Forbid();

            if (errors.Any(e => e.Type == AppErrorType.Conflict))
                return Conflict();

            if (HasValidationErrors(errors))
            {
                AddValidationErrorsToModelState(errors.ToList());
                return BadRequest(ModelState);
            }

            return StatusCode(500);
        }

        protected bool HasValidationErrors(IEnumerable<AppError> errors)
        {
            return errors.Any(e => e.Type == AppErrorType.Validation);
        }

        protected void AddValidationErrorsToModelState(IEnumerable<AppError> errors)
        {
            foreach (var error in errors.Where(e => e.Type == AppErrorType.Validation))
            {
                ModelState.AddModelError(error.PropertyName ?? "", error.Message);
            }
        }

        protected void TriggerToast(
            string message,
            string type = "info")
        {
            Response.Headers["HX-Trigger"] =
                JsonSerializer.Serialize(new
                {
                    showToast = new
                    {
                        message,
                        type
                    }
                });
        }
    }
}
