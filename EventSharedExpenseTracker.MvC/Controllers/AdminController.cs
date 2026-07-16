using EventSharedExpenseTracker.Infrastructure.Seed;
using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public class AdminController : Controller
    {
        //[HttpGet("Admin/SeedDemoData")]
        public async Task<IActionResult> SeedDemoData([FromServices] DemoDataSeeder seeder)
        {
            if (!HttpContext.RequestServices
                    .GetRequiredService<IWebHostEnvironment>()
                    .IsDevelopment())
            {
                return Forbid();
            }

            await seeder.SeedBiDataAsync(new DemoDataOptions
            {
                UserCount = 30,
                TripCount = 80,
                MinOtherExpensesPerDay = 2,
                MaxOtherExpensesPerDay = 10,
                YearsBack = 3
            });

            return RedirectToAction("Index", "Trips");
        }
    }
}
