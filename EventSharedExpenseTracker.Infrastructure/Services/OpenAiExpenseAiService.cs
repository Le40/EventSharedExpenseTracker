using EventSharedExpenseTracker.Application.Common.Interfaces;
using EventSharedExpenseTracker.Domain.Constants;
using EventSharedExpenseTracker.Domain.Enums;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System.Text.Json;

public class OpenAiExpenseAiService : IExpenseAiService
{
    private const string CategoryModel = "gpt-5.4-nano";
    private const string ReceiptModel = "gpt-4.1-mini";

    private readonly string _apiKey;

    public OpenAiExpenseAiService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new InvalidOperationException("OpenAI API key is missing.");
    }

    private ChatClient CreateClient(string model)
    {
        return new ChatClient(model, _apiKey);
    }

    public async Task<CategorySuggestionResponse> SuggestCategoryAsync(
        string expenseName,
        IEnumerable<string> availableCategories)
    {
        var chatClient = CreateClient(CategoryModel);

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

        ChatCompletion completion = await chatClient.CompleteChatAsync(prompt);

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





    public async Task<ReceiptParseResult> ParseReceiptAsync(
        byte[] imageBytes,
        string contentType)
    {
        var chatClient = CreateClient(ReceiptModel);
        var allowedCategories = Enum.GetNames<ExpenseCategory>();

        var prompt = $"""
        Extract expense data from this receipt image.

        Allowed categories:
        {string.Join(", ", allowedCategories)}

        Return only valid JSON with these properties:
        expenseName
        date
        totalAmount
        currencyCode
        suggestedCategory
        confidence

                IMPORTANT:
        - totalAmount must be the FINAL amount paid including VAT, Tip and Taxes.
        - Use the amount next to labels such as:
          TOTAL
          GRAND TOTAL
          AMOUNT DUE
          TO PAY
          SUM
        - Never use individual line item prices.
        - Never use subtotal if a final total exists.
        - If VAT and TOTAL both exist, use TOTAL.
        - for this scan from bottom of the document.

        Rules:
        - expenseName suggest one max lenght is {ExpenseConstants.NameMaxLength}.
        - date must be in yyyy-MM-dd format, or null if not visible
        - totalAmount must be a number, or null if not visible
        - currencyCode must be ISO code like EUR, USD, CZK, or null if not visible
        - suggestedCategory must be exactly one of the allowed categories, or null if unsure
        - confidence must be between 0 and 1
        - do not include markdown
        - do not include explanation outside JSON
        """;

        var messages = new ChatMessage[]
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(prompt),
                ChatMessageContentPart.CreateImagePart(
                    BinaryData.FromBytes(imageBytes),
                    contentType,
                    ChatImageDetailLevel.Low))
        };

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages);

        var json = completion.Content[0].Text;

        try
        {
            var dto = JsonSerializer.Deserialize<ReceiptParseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (dto is null)
                return EmptyResult();

            ExpenseCategory? category = null;

            if (!string.IsNullOrWhiteSpace(dto.SuggestedCategory)
                && Enum.TryParse<ExpenseCategory>(dto.SuggestedCategory, ignoreCase: true, out var parsedCategory))
            {
                category = parsedCategory;
            }

            return new ReceiptParseResult
            {
                Name = dto.ExpenseName,
                Date = DateOnly.TryParse(dto.Date, out var parsedDate) ? parsedDate : null,
                TotalAmount = dto.TotalAmount,
                CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
                Category = category,
                Confidence = Math.Clamp(dto.Confidence ?? 0m, 0m, 1m)
            };
        }
        catch
        {
            return EmptyResult();
        }
    }

    private static ReceiptParseResult EmptyResult()
    {
        return new ReceiptParseResult
        {
            Confidence = 0m
        };
    }

    private static string? NormalizeCurrency(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return null;

        return currencyCode.Trim().ToUpperInvariant();
    }

    private sealed record ReceiptParseDto
    {
        public string? ExpenseName { get; init; }
        public string? Date { get; init; }
        public decimal? TotalAmount { get; init; }
        public string? CurrencyCode { get; init; }
        public string? SuggestedCategory { get; init; }
        public decimal? Confidence { get; init; }
    }
}
