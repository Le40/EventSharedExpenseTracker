using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Infrastructure.Services
{
    public class FakeExpenseAiService : IExpenseAiService
    {
        public Task<CategorySuggestionResponse> SuggestCategoryAsync(
            string expenseName,
            IEnumerable<string> availableCategories)
        {
            var name = expenseName.ToLower();

            var category =
                name.Contains("bolt") || name.Contains("taxi") ? "Tickets" :
                name.Contains("pizza") || name.Contains("restaurant") ? "Food" :
                name.Contains("hotel") ? "Accommodation" :
                "Other";

            return Task.FromResult(new CategorySuggestionResponse(
                category,
                0.8m,
                "Fake local suggestion for testing."));
        }

        public Task<ReceiptParseResult> ParseReceiptAsync(
        byte[] imageBytes,
        string contentType)
        {
            return Task.FromResult(new ReceiptParseResult
            {
                Date = DateOnly.FromDateTime(DateTime.Today),
                TotalAmount = 18.72m,
                CurrencyCode = "EUR",
                Category = ExpenseCategory.Groceries,
                Confidence = 0.95m
            });
        }
    }
}
