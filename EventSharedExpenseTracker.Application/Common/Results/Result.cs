namespace EventSharedExpenseTracker.Application.Common.Results;

public class Result
{
    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyCollection<AppError> Errors { get; }

    protected Result(IEnumerable<AppError> errors)
        => Errors = errors.ToList();


    public static Result Ok() 
        => new ([]);
    public static Result Fail(IEnumerable<AppError> errors) 
        => new (errors);
    public static Result Fail(AppError error) 
        => new ([error]);

    public static implicit operator Result(AppError error) 
        => Fail(error);
    public static implicit operator Result(List<AppError> errors) 
        => Fail(errors);
}


public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base([])
        => Value = value;
    private Result(IEnumerable<AppError> errors) : base(errors)
        => Value = default;
    private Result(AppError error) : base([error])
        => Value = default;
    

    public static Result<T> Ok(T value) 
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




