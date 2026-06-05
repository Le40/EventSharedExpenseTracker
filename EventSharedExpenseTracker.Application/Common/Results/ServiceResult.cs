namespace EventSharedExpenseTracker.Application.Common.Results;

public class ServiceResult
{
    public bool IsSuccess => Errors.Count == 0;
    public IReadOnlyCollection<AppError> Errors { get; }

    protected ServiceResult(IEnumerable<AppError> errors)
        => Errors = errors.ToList();


    public static ServiceResult Ok() 
        => new ([]);
    public static ServiceResult Fail(IEnumerable<AppError> errors) 
        => new (errors);
    public static ServiceResult Fail(AppError error) 
        => new ([error]);

    public static implicit operator ServiceResult(AppError error) 
        => Fail(error);
    public static implicit operator ServiceResult(List<AppError> errors) 
        => Fail(errors);
}


public class ServiceResult<T> : ServiceResult
{
    public T? Value { get; }

    private ServiceResult(T value) : base([])
        => Value = value;
    private ServiceResult(IEnumerable<AppError> errors) : base(errors)
        => Value = default;
    private ServiceResult(AppError error) : base([error])
        => Value = default;
    

    public static ServiceResult<T> Ok(T value) 
        => new(value);
    public static new ServiceResult<T> Fail(IEnumerable<AppError> errors) 
        => new(errors);
    public static new ServiceResult<T> Fail(AppError error)
        => new(error);

    public static implicit operator ServiceResult<T>(T value)
        => Ok(value);
 
    public static implicit operator ServiceResult<T>(AppError error) 
        => Fail(error);
  
    public static implicit operator ServiceResult<T>(List<AppError> errors) 
        => Fail(errors);
    // extra method so i can conveniently change the <T> witout rewrapping errors
    public ServiceResult<T> ToFailure<T>()
        => ServiceResult<T>.Fail(Errors);

}




