using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using EventSharedExpenseTracker.Application.Services.Interfaces;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class FriendsController : Controller
{
    private readonly IFriendService _friendService;

    public FriendsController(IFriendService friendService)
    {
        _friendService = friendService;
    }

    // INDEX : GET
    [HttpGet("Friends/")]
    public async Task<IActionResult> Index()
    {
        var result = await _friendService.Index();
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        var friends = result.Data;

        return PartialView("_Index", friends);
    }

    // SEARCH : GET
    [HttpGet("Friends/Search/")]
    public async Task<IActionResult> Search(string searchString, int? tripId)
    {
        ViewBag.SearchString = searchString;
        if (tripId.HasValue)
            ViewBag.TripId = tripId.Value;

        var result = await _friendService.Search(searchString);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        var users = result.Data;

        return PartialView("_AddParticipant", users);
    }

    // INVITE: POST
    [HttpPost("Friends/Invite/")]
    public async Task<IActionResult> Invite(int friendId)
    {
        var result = await _friendService.Invite(friendId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Index", "Friends");
    }

    // INVITE: ACCEPT : POST
    [HttpPost("Friends/Accept/")]
    public async Task<IActionResult> Accept(int friendshipId)
    {
        var result = await _friendService.Accept(friendshipId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Index", "Friends");
    }

    // INVITE: DECLINE : POST
    [HttpPost("Friends/Decline/")]
    public async Task<IActionResult> Decline(int friendshipId)
    {
        var result = await _friendService.Decline(friendshipId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Index", "Friends");
    }


    // DELETE: POST
    [HttpDelete("Friends/Delete/")]
    public async Task<IActionResult> Delete(int friendshipId)
    {
        var result = await _friendService.Delete(friendshipId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Index", "Friends");
    }


}

