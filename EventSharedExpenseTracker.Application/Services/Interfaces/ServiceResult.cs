using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public class ServiceResult<T>
{



    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public ServiceResult(T data, int statusCode)
    {
        Data = data;
        StatusCode = statusCode;
    }

    public ServiceResult(string errorMessage, int statusCode)
    {
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }
}

public record Result
{
    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyCollection<AppError> Errors { get; }

    protected Result(IEnumerable<AppError> errors)
    {
        Errors = errors.ToList();
    }

    public static Result Success() => new(Array.Empty<AppError>());

    public static Result Fail(IEnumerable<AppError> errors) => new(errors);
    public static Result Fail(AppError error) => new(new[] { error });
}


public record Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(Array.Empty<AppError>())
    {
        Value = value;
    }

    private Result(IEnumerable<AppError> errors) : base(errors)
    {
        Value = default;
    }

    private Result(AppError error) : base(new[] { error })
    {
        Value = default;
    }

    public Result<T> WithErrors(IEnumerable<AppError> errors)
    {
        return new Result<T>(Errors.Concat(errors));
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(IEnumerable<AppError> errors) => new(errors);
    public static Result<T> Fail(AppError error) => new(error);

    public override string ToString()
    {
        return string.Join("; ", Errors.Select(e => e.Message));
    }
}

public enum ErrorType
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    Unexpected
}

public sealed class AppError
{
    public ErrorType Type { get; init; }
    public string Code { get; init; } = "";
    public string Message { get; init; } = "";
    public string? PropertyName { get; init; }
}

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

    public static AppError Validation<T>(string message, string? propertyName = null) => new()
    {
        Type = ErrorType.Validation,
        Code = $"{typeof(T).Name}.Validation.Error",
        Message = message,
        PropertyName = propertyName
    };
}
