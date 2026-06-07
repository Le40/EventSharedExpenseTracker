
using EventSharedExpenseTracker.Domain.Enums;
using System.Net.Mime;

namespace EventSharedExpenseTracker.Application.Common.Interfaces
{
    public interface IExpenseAiService
    {
        Task<CategorySuggestionResponse> SuggestCategoryAsync(
            string expenseName,
            IEnumerable<string> availableCategories);

        Task<ReceiptParseResult> ParseReceiptAsync(
            byte[] imageBytes, string contentType);
    }

    public record CategorySuggestionResponse(
        string SuggestedCategory,
        decimal Confidence,
        string Reason);

    public record ReceiptParseResult
    {
        public string? Name { get; init; }
        public DateOnly? Date { get; init; }
        public decimal? TotalAmount { get; init; }
        public string? CurrencyCode { get; init; }
        public ExpenseCategory? Category { get; init; }
        public decimal Confidence { get; init; }
    }
}
