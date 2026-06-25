using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public class HealthController : Controller
    {
        [HttpGet("/health/ping")]
        public IActionResult Ping()
        {
            return Ok("ok");
        }
    }
}
