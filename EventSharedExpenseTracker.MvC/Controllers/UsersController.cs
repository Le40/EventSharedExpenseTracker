using EventSharedExpenseTracker.Application.Authorisation;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers
{
    public class UserController : Controller
    {
        // also added UserController, and not yet remember if i need to.
        // Unitofwork here, so skiped service, just to test if search will work, later, needed to be made into user service, to maintain SoC.
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            
        }

        /*public async Task<IActionResult> ListUsers()
        {
            List<CustomUser> users = await _unitOfWork.Users.GetAllAsync();
            return View(users);
        }*/
    }
}
