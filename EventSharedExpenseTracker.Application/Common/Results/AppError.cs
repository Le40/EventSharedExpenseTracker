
namespace EventSharedExpenseTracker.Application.Common.Results
{
    public sealed class AppError
    {
        public ErrorType Type { get; init; }
        public string Code { get; init; } = "";
        public string Message { get; init; } = "";
        public string? PropertyName { get; init; }
    }
}
