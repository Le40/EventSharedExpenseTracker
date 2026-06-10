
namespace EventSharedExpenseTracker.Application.Common.Interfaces
{
    public interface IExpenseAiService
    {
        Task<CategorySuggestionResponse> SuggestCategoryAsync(
            string expenseName,
            IEnumerable<string> availableCategories);
    }

    public record CategorySuggestionResponse(
        string SuggestedCategory,
        decimal Confidence,
        string Reason);
}
