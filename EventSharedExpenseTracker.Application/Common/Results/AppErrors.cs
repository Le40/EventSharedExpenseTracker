
namespace EventSharedExpenseTracker.Application.Common.Results
{
    public static class AppErrors
    {
        public static AppError NotFound<T>() => new()
        {
            Type = ErrorType.NotFound,
            Code = $"{typeof(T).Name}.NotFound",
            Message = $"{typeof(T).Name} was not found."
        };

        public static AppError Forbidden<T>() => new()
        {
            Type = ErrorType.Forbidden,
            Code = $"{typeof(T).Name}.Forbidden",
            Message = $"Insufficient permissions."
        };

        public static AppError Conflict<T>() => new()
        {
            Type = ErrorType.Conflict,
            Code = $"{typeof(T).Name}.Conflict",
            Message = $"Bad Request."
        };

        public static AppError Validation<T>(string message, string? propertyName = null) => new()
        {
            Type = ErrorType.Validation,
            Code = $"{typeof(T).Name}.Validation.Error",
            Message = message,
            PropertyName = propertyName
        };

    }

}
