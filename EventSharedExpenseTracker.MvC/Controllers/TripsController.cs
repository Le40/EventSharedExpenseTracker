using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.MvC.ActionFilters;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
public class TripsController : Controller
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    // INDEX
    [Route("Trips/")]
    [Route("/")]
    public async Task<IActionResult> Index(string sortOrder, string searchString, string categoryFilter)
    {
        ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
        ViewBag.SearchString = searchString;
        ViewBag.CategoryFilter = categoryFilter;

        var result = await _tripService.Index(sortOrder, searchString, categoryFilter);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return View(result.Data);
    }

    // DETAILS
    [Route("Trips/Details/{tripId}")]
    public async Task<IActionResult> Details(int tripId)
    {
        ViewBag.TripId = tripId;
        var result = await _tripService.Details(tripId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return View(result.Data);
    }

    // CREATE: GET
    [Route("Trips/Create")]
    public IActionResult Create()
    {
        return PartialView("_Create");
    }

    // CREATE: POST
    [HttpPost]
    [Route("Trips/Create")]
    [ValidateAntiForgeryToken]
    [ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Create([Bind("Id,Name,DateFrom,DateTo,CreatorId")] Trip trip, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            HttpContext.Response.Headers.Append("Hx-Retarget", "#createTrip");
            return StatusCode(400,PartialView("_Create", trip));
        }

        Stream? imageStream = null;
        if (imageFile != null)
            imageStream = imageFile.OpenReadStream();
  
        var result = await _tripService.Add(trip, imageStream);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction(nameof(Index));
    }


    // EDIT: GET
    [Route("Trips/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _tripService.Get(id);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return PartialView("_Edit", result.Data);
    }


    // EDIT: POST
    [HttpPost]
    [Route("Trips/Edit/{id}")]
    [ValidateAntiForgeryToken]
    [ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,DateFrom,DateTo,CreatorId,ImagePath")] Trip trip, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            HttpContext.Response.Headers.Append("Hx-Retarget", "#tripEdit");
            return StatusCode(400, PartialView("_Edit", trip));
        }

        Stream? imageStream = null;
        if (imageFile != null)
            imageStream = imageFile.OpenReadStream();

        var result = await _tripService.Update(trip, imageStream);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction(nameof(Details), new { id = trip.Id });
    }

    // DELETE: POST
    [HttpDelete]
    [ValidateAntiForgeryToken]
    [Route("Trips/Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tripService.Delete(id);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Trips/{id}/AddParticipant/{friendId}")]
    public async Task<IActionResult> AddParticipant(int id, int friendId)
    {
        var result = await _tripService.AddParticipant(id, friendId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Trips/{id}/AddDummy/")]
    public async Task<IActionResult> AddDummy(int id, string searchString)
    {
        var result = await _tripService.AddDummy(id, searchString);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { id });
    }

    [HttpDelete]
    [ValidateAntiForgeryToken]
    [Route("Trips/{id}/DeleteParticipant/{participantId}")]
    public async Task<IActionResult> DeleteParticipant(int id, int participantId)
    {
        var result = await _tripService.DeleteParticipant(id, participantId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { id });
    }
}
