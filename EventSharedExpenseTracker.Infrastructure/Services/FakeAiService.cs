using EventSharedExpenseTracker.Application.Common.Interfaces;

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
    }
}
