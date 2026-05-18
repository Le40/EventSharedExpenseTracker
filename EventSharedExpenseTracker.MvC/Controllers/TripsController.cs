using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Trips;
using EventSharedExpenseTracker.Application.Trips.DTOs;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Mappers.Trips;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Trips;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static EventSharedExpenseTracker.MvC.ViewModels.Expenses.ExpenseFormViewModel;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class TripsController : BaseController
{
    private readonly ITripService _tripService;
    private readonly IRequestContext _requestContext;

    public TripsController(ITripService tripService, IRequestContext requestContext)
    {
        _tripService = tripService;
        _requestContext = requestContext;
    }

    // INDEX
    [HttpGet("Trips/")]
    [HttpGet("/")]
    public async Task<IActionResult> Index(string? sortOrder, string? searchString, TripCategory? categoryFilter)
    {
        ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
        ViewBag.SearchString = searchString;
        ViewBag.CategoryFilter = categoryFilter;

        var result = await _tripService.Index(sortOrder, searchString, categoryFilter);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var models = new TripIndexViewModel
        {
            Creator = true,
            Trips = result.Value.Select(r => r.Adapt<TripIndexItemViewModel>()).ToList()
        };

        return View(models);
    }

    // DETAILS
    [HttpGet("Trips/Details/{id}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        var result = await _tripService.Details(id);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var tripDetailsQuery = result.Value!;

        var model = TripDetailsMapper.FromQuery(tripDetailsQuery);
      
        return View(model);
    }

    // CREATE: GET
    [HttpGet("Trips/Create")]
    public IActionResult Create()
    {
        return PartialView("_TripFormCreate", new TripFormViewModel());
    }

    // CREATE: POST
    [HttpPost("Trips/Create")]
    public async Task<IActionResult> Create(TripFormViewModel model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return RenderTripForm(model, TripFormMode.Create);

        await using var imageStream = imageFile?.OpenReadStream();

        var command = model.Adapt<TripCommand>();
  
        var result = await _tripService.Add(command, imageStream);

        if (!result.IsSuccess)
            return ReturnFormOrError(result, model, TripFormMode.Create);

        return RedirectToAction(nameof(Index));
    }

    // EDIT: GET
    [HttpGet("Trips/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _tripService.GetTripForm(id);
        if (!result.IsSuccess)
        {
            return HandleServiceErrors(result.Errors);
        }

        var model = result.Value.Adapt<TripFormViewModel>();

        return RenderTripForm(model, TripFormMode.Edit);
    }

    // EDIT: POST
    [HttpPost("Trips/Edit/{id}")]
    public async Task<IActionResult> Edit([FromRoute] int id, TripFormViewModel model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return RenderTripForm(model, TripFormMode.Edit);

        await using var imageStream = imageFile?.OpenReadStream();
        var command = model.Adapt<TripCommand>();
        var result = await _tripService.Update(id, command, imageStream);

        if (!result.IsSuccess)
            return ReturnFormOrError(result, model, TripFormMode.Edit);

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // DELETE: POST
    [HttpPost("Trips/Delete/{id}")]
    // post, because with delete redirect doesnt work, post method also in the View
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tripService.Delete(id);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        return RedirectToAction(nameof(Index));
    }

    // ADD PARTICIPANT: POST
    [HttpPost("Trips/{id}/AddParticipant/{friendId}")]
    public async Task<IActionResult> AddParticipant(int id, int friendId)
    {
        var result = await _tripService.AddParticipant(id, friendId);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        return RedirectToAction("Details", "Trips", new { id });
    }

    // ADD DUMMY: POST
    [HttpPost("Trips/{id}/AddDummy/")]
    public async Task<IActionResult> AddDummy(int id, string searchString)
    {
        var result = await _tripService.AddDummy(id, searchString);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        return RedirectToAction("Details", "Trips", new { id });
    }

    // DELETE PARTICIPANT
    [HttpPost("Trips/{id}/DeleteParticipant/{participantId}")]
    public async Task<IActionResult> DeleteParticipant(int id, int participantId)
    {
        var result = await _tripService.DeleteParticipant(id, participantId);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);
        return RedirectToAction("Details", "Trips", new { id });
    }

    private IActionResult ReturnFormOrError(Result result, TripFormViewModel model, TripFormMode mode)
    {
        if (TryAddValidationErrorsToModelState(result.Errors))
            return RenderTripForm(model, mode);

        return HandleServiceErrors(result.Errors);
    }

    private PartialViewResult RenderTripForm(TripFormViewModel model, TripFormMode mode)
    {
        // Retarget, so if form is not valid it will target just #createTrip in View to return validation errors, but if its valid it will return whole body with new trip.
        //return StatusCode(400, PartialView("_Edit", trip));
        // htmx wont return 400+ code back to dom, so its 200 even for bad form.

        model.Mode = mode;

        Response.Headers.Append("Hx-Retarget", $"#{model.ElementId}");
        return PartialView($"_TripForm{mode}", model);
    }
}

