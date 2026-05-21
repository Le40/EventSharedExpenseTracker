using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Domain.Result
{
    public sealed record DomainError(
        DomainErrorType Type,
        string Code,
         string Message
    );

}
