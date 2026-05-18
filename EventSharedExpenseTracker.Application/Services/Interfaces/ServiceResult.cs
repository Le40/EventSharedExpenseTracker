using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EventSharedExpenseTracker.Application.Services.Interfaces;

public class Result
{
    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyCollection<AppError> Errors { get; }

    protected Result(IEnumerable<AppError> errors)
        =>Errors = errors.ToList();


    public static Result Ok() 
        => new Result(Array.Empty<AppError>());
    public static Result Fail(IEnumerable<AppError> errors) 
        => new Result(errors);
    public static Result Fail(AppError error) 
        => new Result (new[] { error });

    public static implicit operator Result(AppError error) 
        => Fail(error);
    public static implicit operator Result(List<AppError> errors) 
        => Fail(errors);
}


public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base(Array.Empty<AppError>())
        => Value = value;
    private Result(IEnumerable<AppError> errors) : base(errors)
        => Value = default;
    private Result(AppError error) : base(new[] { error })
        => Value = default;
    

    public static new Result<T> Ok(T value) 
        => new(value);
    public static new Result<T> Fail(IEnumerable<AppError> errors) 
        => new(errors);
    public static new Result<T> Fail(AppError error)
        => new(error);

    public static implicit operator Result<T>(T value)
        => Ok(value);
 
    public static implicit operator Result<T>(AppError error) 
        => Fail(error);
  
    public static implicit operator Result<T>(List<AppError> errors) 
        => Fail(errors);
  
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
