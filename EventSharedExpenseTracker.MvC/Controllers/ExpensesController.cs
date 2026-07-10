using EventSharedExpenseTracker.Application.Common.Results;
using EventSharedExpenseTracker.Application.Expenses;
using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.MvC.Common;
using EventSharedExpenseTracker.MvC.Factories;
using EventSharedExpenseTracker.MvC.Mappers.Expenses;
using EventSharedExpenseTracker.MvC.ViewModels.Expenses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> SuggestCategory(string name, string formId, ExpenseCategory? category)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length <=3)
            return NoContent();

        // User already chose a category
        if (category.HasValue)
            return NoContent();

        var resultAi = await _expenseService.SuggestCategoryAsync(name);
        if (!resultAi.IsSuccess)
            return HandleServiceErrors(resultAi.Errors);
        var suggestion = resultAi.Value!;

        var vm = new CategorySelectViewModel
        {
            FormId = formId,
            SelectedCategory = suggestion
        };

        return PartialView("_ExpenseForm_CategorySelect", vm);
    }

    [HttpPost]
    public async Task<IActionResult>ParseReceipt(int tripId, IFormFile receiptImage)
    {
        if (receiptImage is null || receiptImage.Length == 0)
            return BadRequest("Receipt image is required.");

        using var stream = receiptImage.OpenReadStream();
        var resultAi = await _expenseService.ExtractReceiptDataAsync(stream);
        if (!resultAi.IsSuccess)
            return HandleServiceErrors(resultAi.Errors);

        var parsedReceipt = resultAi.Value!;

        var result = await _expenseFormFactory.BuildCreateFromReceiptAsync(tripId, parsedReceipt);

        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var vm = result.Value!;

        return PartialView("_ExpenseForm", vm);
    }


    // for offline parsing, that needs json to map to offline draft
    // reason is, offline save receipts can be parsed without user intervention in the background without saving them as expenses.
    [HttpPost]
    public async Task<IActionResult> ParseReceiptJson(int tripId, IFormFile receiptImage)
    {
        if (receiptImage is null || receiptImage.Length == 0)
            return BadRequest("Receipt image is required.");

        var result = await _expenseService.ExtractReceiptDataAsync(receiptImage.OpenReadStream());

        if (!result.IsSuccess)
            return BadRequest(new { errors = result.Errors.Select(e => e.Message) });

        var parsed = result.Value;

        return Ok(result.Value);
    }

    // EXPENSES : INDEX
    [HttpGet("Trips/{tripId}/Expenses/")]
    public async Task<IActionResult> Index(int tripId, string? sortOrder, string? searchString, ExpenseCategory? categoryFilter, bool creator = false)
    {
        var result = await _expenseService.GetIndex(tripId, sortOrder, searchString, creator, categoryFilter);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        var tripCurrencyCode = result.Value.BaseCurrencyCode;
        var vm = new ExpenseIndexViewModel
        {
            Expenses = result.Value!.Expenses.Select(e => ExpenseVMMapper.FromQuery(e, tripCurrencyCode)).ToList(),
            TripId = tripId,
            SearchString = searchString,
            //CategoryFilter = categoryFilter,
            //Creator = creator,
            //CurrentSort = sortOrder,
            //NameSortParam = sortOrder == "name" ? "name_desc" : "name",
            //DateSortParam = sortOrder == "date" ? "date_desc" : "date",
            //AmountSortParam = sortOrder == "amount" ? "amount_desc" : "amount",
            BaseCurrencyCode = result.Value.BaseCurrencyCode
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
            return FormValidationResponse(model, ExpenseFormMode.Create);

        var expenseCommand = ExpenseVMMapper.ToCommand(model);//, _requestContext.UserId

        var result = await _expenseService.Add(expenseCommand, tripId);

        if (!result.IsSuccess)
            return ServiceErrorResponse(result, model, ExpenseFormMode.Create);

        Response.Headers["HX-Redirect"] = Url.Action("Details", "Trips", new { id = model.TripId });
        return new EmptyResult();
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
            return ServiceErrorResponse(result, model, ExpenseFormMode.Edit);

        Response.Headers["HX-Redirect"] = Url.Action("Details", "Trips", new { id = model.TripId });
        return new EmptyResult();
    }

    // DELETE: POST
    [HttpPost("Expenses/Delete/{id}/")]
    // post, because with delete redirect doesnt work, post method also in the View
    public async Task<IActionResult> Delete(int id, int tripId)
    {
        var result = await _expenseService.Delete(id);
        if (!result.IsSuccess)
            return HandleServiceErrors(result.Errors);

        Response.Headers["HX-Redirect"] = Url.Action("Details", "Trips", new { id = tripId });
        return new EmptyResult();
    }

    private PartialViewResult RenderExpenseForm(ExpenseFormViewModel model, ExpenseFormMode mode)
    {
        model.Mode = mode;
        model.CurrencyOptions = CurrencySelectList.Get("EUR");

        return PartialView("_ExpenseForm", model);
    }

    private IActionResult FormValidationResponse(ExpenseFormViewModel model, ExpenseFormMode mode)
    {
        if (IsOfflineSync())
            return OfflineValidation();

        return RenderExpenseForm(model, mode);
    }

    private IActionResult ServiceErrorResponse(ServiceResult result, ExpenseFormViewModel model, ExpenseFormMode mode)
    {
        if (HasValidationErrors(result.Errors))
        {
            AddValidationErrorsToModelState(result.Errors);
            return FormValidationResponse(model, mode);
        }

        return HandleServiceErrors(result.Errors);
    }
    private bool IsOfflineSync()
    {
        return Request.Form["IsOfflineSync"] == "true";
    }

    private IActionResult OfflineValidation()
    {
        return BadRequest(new
        {
            Errors = ModelState
                .Where(x => x.Value?.Errors.Any() == true)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage))
        });
    }
}
