using EventSharedExpenseTracker.Application.Dtos;
using EventSharedExpenseTracker.Application.Dtos.Mappers;
using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.Mappers.Trips;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Trips;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> Index(string? sortOrder, string? searchString, string? categoryFilter)
    {
        ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
        ViewBag.SearchString = searchString;
        ViewBag.CategoryFilter = categoryFilter;

        var result = await _tripService.Index(sortOrder, searchString, categoryFilter);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var models = result.Value.Select(r => r.Adapt<TripIndexItemViewModel>()).ToList();

        return View(models);
    }

    // DETAILS
    [HttpGet("Trips/Details/{id}")]
    public async Task<IActionResult> Details([FromRoute] int id)
    {
        var userId = _requestContext.UserId;

        //ViewBag.TripId = tripId;
        var result = await _tripService.Details(id);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var tripDetailsQuery = result.Value!;

        var model = TripDetailsMapper.FromQuery(tripDetailsQuery);
        model.ExpenseIndex.TripId = id;
      
        return View(model);
    }

    // CREATE: GET
    [HttpGet("Trips/Create")]
    public IActionResult Create()
    {
        return PartialView("_Create", new TripFormViewModel());
    }

    // CREATE: POST
    [HttpPost("Trips/Create")]
    //[ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Create(TripFormViewModel model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return ReturnCreateForm(model);

        await using var imageStream = imageFile?.OpenReadStream();

        var command = model.Adapt<TripCommand>();
  
        var result = await _tripService.Add(command, imageStream);
        if (!result.IsSuccess)
        {
            var validationErrors = result.Errors
                .Where(e => e.Type == ErrorType.Validation)
                .ToList();

            if (validationErrors.Any())
            {
                AddErrorsToModelState(validationErrors);
                return ReturnCreateForm(model);
            }
            return HandleServiceErrors(result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }

    // EDIT: GET
    [HttpGet("Trips/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _tripService.GetForUpdate(id);
        if (!result.IsSuccess)
        {
            return HandleServiceErrors(result.Errors);
        }

        var vm = result.Value.Adapt<TripFormViewModel>();

        return PartialView("_Edit", vm);
    }

    // EDIT: POST
    [HttpPost("Trips/Edit/{id}")]
    //[ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Edit([FromRoute] int id, TripFormViewModel model, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
            return ReturnEditForm(model);

        await using var imageStream = imageFile?.OpenReadStream();
        model.Id = id;
        var command = model.Adapt<TripCommand>();

        var result = await _tripService.Update(command, imageStream);
        if (!result.IsSuccess)
        {
            var validationErrors = result.Errors
                .Where(e => e.Type == ErrorType.Validation)
                .ToList();

            if (validationErrors.Any())
            {
                AddErrorsToModelState(validationErrors);
                return ReturnEditForm(model);
            }
            return HandleServiceErrors(result.Errors);
        }

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
        //return View("Details", id);
        return RedirectToAction("Details", "Trips", new { id });
    }

    private IActionResult ReturnCreateForm(TripFormViewModel model)
    {
        HttpContext.Response.Headers.Append("Hx-Retarget", "#createTrip");
        // Retarget, so if form is not valid it will target just #createTrip in View to return validation errors, but if its valid it will return whole body with new trip.
        //return StatusCode(400,PartialView("_Create", trip));
        // htmx wont return 400+ code back to dom, so its 200 even for bad form.
        return PartialView("_Create", model);
    }

    private IActionResult ReturnEditForm(TripFormViewModel model)
    {
        HttpContext.Response.Headers.Append("Hx-Retarget", "#tripEdit");
        // Retarget, so if form is not valid it will target just #createTrip in View to return validation errors, but if its valid it will return whole body with new trip.
        //return StatusCode(400, PartialView("_Edit", trip));
        // htmx wont return 400+ code back to dom, so its 200 even for bad form.
        return PartialView("_Edit", model);
    }
}
