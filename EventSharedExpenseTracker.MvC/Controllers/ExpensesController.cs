using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using EventSharedExpenseTracker.Domain.Models;
using EventSharedExpenseTracker.Application.Services.Interfaces;
using EventSharedExpenseTracker.MvC.ActionFilters;

namespace EventSharedExpenseTracker.MvC.Controllers;

[Authorize]
[AutoValidateAntiforgeryToken]
public class ExpensesController : Controller
{
    private readonly IExpenseService _expenseService;

    public ExpensesController(IExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    // EXPENSES : INDEX
    [Route("Trips/{tripId}/Expenses/")]
    public async Task<IActionResult> Index(int tripId, string sortOrder, string searchString, string creator, string categoryFilter)
    {
        ViewBag.TripId = tripId;
        ViewBag.NameSortParam = sortOrder == "name" ? "name_desc" : "name";
        ViewBag.DateSortParam = sortOrder == "date" ? "date_desc" : "date";
        ViewBag.AmmSortParam = sortOrder == "amount" ? "amount_desc" : "amount";
        ViewBag.Creator = creator == "on" ? "off" : "on";
        ViewBag.SearchString = searchString;
        ViewBag.CategoryFilter = categoryFilter;

        var result = await _expenseService.Index(tripId, sortOrder, searchString, creator, categoryFilter);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        var expenses = result.Data;

        string? hxHeader = Request.Headers["HX-Target"];

        if (hxHeader != null)
            return PartialView("_Index", expenses);

        return View(expenses);
    }

    // CREATE: GET
    [Route("Expenses/Add/")]
    public async Task<IActionResult> Create(int tripId)
    {
        var result = await _expenseService.Create(tripId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        ViewBag.Action = "Create";
        return PartialView("_Form", result.Data);
        //return await RenderExpenseForm(result.Data, "", "Create");
    }

    // CREATE: POST
    [HttpPost]
    [Route("Expenses/Add/")]
    [ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Create(int tripId, [Bind("Id,Name,Date,Category,TripId,CreatorId,Payments")] Expense expense)
    {
        if (!ModelState.IsValid)
        {
            expense = await _expenseService.LoadParticipants(expense);
            ViewBag.Action = "Create";
            HttpContext.Response.Headers.Append("Hx-Retarget", "#createExpense");
            return StatusCode(400, PartialView("_Form", expense));
        }
            //return StatusCode(400, await RenderExpenseForm(expense, "#createExpense", "Create"));

        var result = await _expenseService.Add(expense,tripId);

        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { id = expense.TripId });
    }

    // EDIT: GET
    [Route("Expenses/Edit/{id}")]
    public async Task<IActionResult> Edit(int tripId, int id)
    {
        var result = await _expenseService.Get(id,tripId);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        ViewBag.Action = "Edit";
        return PartialView("_Form", result.Data);
        //return await RenderExpenseForm(result.Data, "", "Edit");
    }

    // EDIT: POST
    [HttpPost]
    [Route("Expenses/Edit/{id}/")]
    [ServiceFilter(typeof(CustomValidationActionFilter))]
    public async Task<IActionResult> Edit(int tripId, int id, [Bind("Id,Name,Date,Category,TripId, CreatorId, Payments")] Expense expense)
    {
        if (!ModelState.IsValid)
        {
            expense = await _expenseService.LoadParticipants(expense);
            ViewBag.Action = "Edit";
            HttpContext.Response.Headers.Append("Hx-Retarget", "#expense" + expense.Id);
            return StatusCode(400, PartialView("_Form", expense));
        }
            //return StatusCode(400, await RenderExpenseForm(expense, "#expense" + expense.Id, "Edit"));

        var result = await _expenseService.Update(expense);

        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { id = expense.TripId });
    }


    // DELETE: POST
    [HttpDelete]
    [Route("Expenses/Delete/{id}/")]
    public async Task<IActionResult> Delete(int id, int tripId)
    {
        var result = await _expenseService.Delete(id);
        if (result.StatusCode != 200)
        {
            return StatusCode(result.StatusCode, result.ErrorMessage);
        }

        return RedirectToAction("Details", "Trips", new { tripId = tripId });
    }

    //private async Task<IActionResult> RenderExpenseForm(Expense expense, string hxTargetElement, string action)
    //{
    //    if (expense.Payments[0].Participant == null)
    //    {
    //        expense = await _expenseService.LoadParticipants(expense);
    //    }

    //    ViewBag.Action = action;
    //    if (hxTargetElement != "")
    //    {
    //        HttpContext.Response.Headers.Append("Hx-Retarget", hxTargetElement);
    //    }
    //    return PartialView("~/Views/Partials/_expenseForm.cshtml", expense);
    //}
}
