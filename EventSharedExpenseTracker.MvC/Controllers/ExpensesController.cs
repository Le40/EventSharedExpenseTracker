using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.Factories;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static EventSharedExpenseTracker.MvC.ViewModels.Expenses.ExpenseFormViewModel;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
public class ExpensesController : BaseController
{
    private readonly IExpenseService _expenseService;
    private readonly ExpenseFormFactory _expenseFormFactory;

    public ExpensesController(IExpenseService expenseService, ExpenseFormFactory expenseFormFactory)
    {
        _expenseService = expenseService;
        _expenseFormFactory = expenseFormFactory;
    }

    // EXPENSES : INDEX
    [HttpGet("Trips/{tripId}/Expenses/")]
    public async Task<IActionResult> Index(int tripId, string? sortOrder, string? searchString, ExpenseCategory? categoryFilter, bool creator = false)
    {
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

        // if the request is from htmx, return the partial view
        if (Request.Headers["HX-Request"] == "true")
            return PartialView("_ExpenseIndex", vm);
        // normal view if in future standalone expense page.
        return View(vm);
    }

    // CREATE: GET
    [HttpGet("Trips/{tripId}/Expenses/Add/")]
    public async Task<IActionResult> Create(int tripId)
    {
        var result = await _expenseFormFactory.BuildCreateAsync(tripId);

        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var model = result.Value;
        return RenderExpenseForm(model!, ExpenseFormMode.Create);
    }

    // CREATE: POST
    [HttpPost("Trips/{tripId}/Expenses/Add/")]
    public async Task<IActionResult> Create([FromRoute] int tripId, ExpenseFormViewModel model)
    {
        if (!ModelState.IsValid)
            return RenderExpenseForm(model, ExpenseFormMode.Create);

        var expenseCommand = ExpenseVMMapper.ToCommand(model);//, _requestContext.UserId

        var result = await _expenseService.Add(expenseCommand, tripId);

        if (!result.IsSuccess)
            return ReturnFormOrError(result, model, ExpenseFormMode.Create);

        return RedirectToAction("Details", "Trips", new { id = tripId });
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

        var model = result.Value;
        return RenderExpenseForm(model!, ExpenseFormMode.Edit);
    }

    // EDIT: POST
    [HttpPost("Expenses/Edit/{id}/")]
    public async Task<IActionResult> Edit(int id, ExpenseFormViewModel model)
    {
        if (!ModelState.IsValid)
            return RenderExpenseForm(model, ExpenseFormMode.Edit);

        var expenseCommand = ExpenseVMMapper.ToCommand(model);//, _requestContext.UserId

        var result = await _expenseService.Update(id, expenseCommand);

        if (!result.IsSuccess)
            return ReturnFormOrError(result, model, ExpenseFormMode.Edit);
 
        return RedirectToAction("Details", "Trips", new { id = result.Value!.TripId });
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

    private IActionResult ReturnFormOrError(ServiceResult result, ExpenseFormViewModel model, ExpenseFormMode mode)
    {
        if (TryAddValidationErrorsToModelState(result.Errors))
            return RenderExpenseForm(model, mode);

        return HandleServiceErrors(result.Errors);
    }

    private PartialViewResult RenderExpenseForm(ExpenseFormViewModel model, ExpenseFormMode mode)
    {
        model.Mode = mode;
        model.CurrencyOptions = CurrencySelectList.Get("EUR");

        Response.Headers.Append("Hx-Retarget", $"#{model.ElementId}");
        return PartialView("_ExpenseForm", model);
    }
}
