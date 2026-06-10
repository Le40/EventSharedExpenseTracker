using EventSharedExpenseTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text.Json;

public class OpenAiExpenseAiService : IExpenseAiService
{
    private readonly ChatClient _chatClient;

    public OpenAiExpenseAiService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key is missing.");

        _chatClient = new ChatClient(
            model: "gpt-5.4-nano",
            apiKey: apiKey);
    }

    public async Task<CategorySuggestionResponse> SuggestCategoryAsync(
        string expenseName,
        IEnumerable<string> availableCategories)
    {
        var categories = availableCategories.ToArray();

        var prompt = $"""
        Suggest one expense category.

        Expense name:
        {expenseName}

        Allowed categories:
        {string.Join(", ", categories)}

        Choose exactly one category from the allowed categories.
        Do not invent new categories.
        Do not explain outside JSON.

        Return only valid JSON with properties:
        suggestedCategory
        confidence
        reason

        If unsure, use "Other".
        """;

        ChatCompletion completion = await _chatClient.CompleteChatAsync(prompt);

        var json = completion.Content[0].Text;

        try
        {
            var result = JsonSerializer.Deserialize<CategorySuggestionResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result is null || !categories.Contains(result.SuggestedCategory))
            {
                return new CategorySuggestionResponse(
                    "Other",
                    0.0m,
                    "AI returned invalid or empty category.");
            }

            return result;
        }
        catch
        {
            return new CategorySuggestionResponse(
                "Other",
                0.0m,
                "Could not parse AI response.");
        }
    }
}
