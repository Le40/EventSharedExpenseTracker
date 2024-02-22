
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
