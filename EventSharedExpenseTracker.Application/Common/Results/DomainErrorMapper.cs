using EventSharedExpenseTracker.Domain.Enums;
using EventSharedExpenseTracker.Domain.Result;

namespace EventSharedExpenseTracker.Application.Common.Results
{
    public static class DomainErrorMapper
    {
        public static AppError ToAppError(DomainError error)
        {
            return new AppError
            {
                Code = error.Code,
                Message = error.Message,

                Type = error.Type switch
                {
                    DomainErrorType.NotFound => AppErrorType.NotFound,
                    DomainErrorType.Validation => AppErrorType.Validation,
                    DomainErrorType.Conflict => AppErrorType.Conflict,
                    _ => AppErrorType.Unexpected
                }
            };
        }

        public static List<AppError> ToAppErrors(IEnumerable<DomainError> errors)
        {
            return errors.Select(ToAppError).ToList();
        }
    }
}
