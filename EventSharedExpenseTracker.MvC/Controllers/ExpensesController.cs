using EventSharedExpenseTracker.Application.Interfaces;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.MvC.Factories;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;
    private readonly IRequestContext _requestContext;
    private readonly ExpenseFormFactory _expenseFormFactory;

    public ExpensesController(IExpenseService expenseService,  IRequestContext requestContext, ExpenseFormFactory expenseFormFactory)
    {
        _expenseService = expenseService;
        _requestContext = requestContext;
        _expenseFormFactory = expenseFormFactory;
    }

    // EXPENSES : INDEX
    [HttpGet("Trips/{tripId}/Expenses/")]
    public async Task<IActionResult> Index(int tripId, string? sortOrder, string? searchString, string? categoryFilter, bool creator = false)
    {
        /*ViewBag.TripId = tripId;
        ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
        ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
        ViewBag.AmmSortParam = sortOrder == "amount" ? "amount_desc" : "amount";
        //ViewBag.Creator = creator == "on" ? "off" : "on";
        ViewBag.Creator = creator;
        ViewBag.SearchString = searchString;
        ViewBag.CategoryFilter = categoryFilter;*/

        var result = await _expenseService.Index(tripId, sortOrder, searchString, creator, categoryFilter);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var vm = new ExpenseIndexViewModel
        {
            Expenses = result.Value!.Select(e => ExpenseVMMapper.FromQuery(e)).ToList(),
            TripId = tripId,
            SearchString = searchString,
            CategoryFilter = categoryFilter,
            Creator = creator,
            CurrentSort = sortOrder,
            NameSortParam = sortOrder == "name" ? "name_desc" : "name",
            DateSortParam = sortOrder == "date" ? "date_desc" : "date",
            AmmSortParam = sortOrder == "amount" ? "amount_desc" : "amount"
        };

        // if the request is from htmx, return the partial view, otherwise return the full view
        // meaning it wants to filter, search or sort.
        if (Request.Headers["HX-Target"].Any())
            return PartialView("_Index", vm);

        return View(vm);
    }

    // CREATE: GET
    [HttpGet("Trips/{tripId}/Expenses/Add/")]
    public async Task<IActionResult> Create(int tripId)
    {
        var result = await _expenseFormFactory.BuildCreateAsync(tripId);

        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        ViewBag.Action = "Create";
        return PartialView("_Form", result.Value);
    }

    // CREATE: POST
    [HttpPost("Trips/{tripId}/Expenses/Add/")]
    public async Task<IActionResult> Create([FromRoute] int tripId, ExpenseFormViewModel model)
    {
        if (!ModelState.IsValid)
            return ReturnCreateForm(model);

        var expenseCommand = ExpenseVMMapper.ToCommand(model, tripId);//, _requestContext.UserId

        var result = await _expenseService.Add(expenseCommand, tripId);

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

        return RedirectToAction("Details", "Trips", new { id = expenseCommand.TripId });
    }

    // EDIT: GET
    [HttpGet("Expenses/Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _expenseFormFactory.BuildEditAsync(id);
        if (!result.IsSuccess)
        {
            return HandleServiceErrors(result.Errors);
        }

        ViewBag.Action = "Edit";
        return PartialView("_Form", result.Value);
    }

    // EDIT: POST
    [HttpPost("Expenses/Edit/{id}/")]
    public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
    {
        if (!ModelState.IsValid)
            return ReturnEditForm(model);

        var expenseCommand = ExpenseVMMapper.ToCommand(model, model.TripId);//, _requestContext.UserId

        var result = await _expenseService.Update(expenseCommand);

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

        return RedirectToAction("Details", "Trips", new { id = expenseCommand.TripId });
    }

    // DELETE: POST
    [HttpPost("Expenses/Delete/{id}/")]
    // post, because with delete redirect doesnt work, post method also in the View
    public async Task<IActionResult> Delete(int id, int tripId)
    {
        var result = await _expenseService.Delete(id);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        return RedirectToAction("Details", "Trips", new { id = tripId });
    }

    private IActionResult ReturnCreateForm(ExpenseFormViewModel model)
    {
        ViewBag.Action = "Create";
        Response.Headers.Append("Hx-Retarget", "#createExpense");
        return PartialView("_Form", model);
    }

    private IActionResult ReturnEditForm(ExpenseFormViewModel model)
    {
        ViewBag.Action = "Edit";
        Response.Headers.Append("Hx-Retarget", "#expense" + model.Id);
        return PartialView("_Form", model);
    }
}
