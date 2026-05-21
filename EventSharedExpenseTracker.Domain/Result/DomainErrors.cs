
using EventSharedExpenseTracker.Domain.Enums;

namespace EventSharedExpenseTracker.Domain.Result
{
    public static class DomainErrors
    {
        public static DomainError NotFound<T>() =>
            new(DomainErrorType.NotFound,
                $"{typeof(T).Name}.NotFound",
                $"{typeof(T).Name} not found.");

        public static readonly DomainError ParticipantAlreadyExists =
            new(DomainErrorType.Conflict,
                "Trip.ParticipantAlreadyExists",
                "Participant already exists.");

        public static readonly DomainError ParticipantNameRequired =
            new(DomainErrorType.Validation,
                "Trip.ParticipantNameRequired",
                "Participant name is required.");

        public static readonly DomainError InvalidTripDateRange =
            new(DomainErrorType.Validation,
                "Trip.InvalidDateRange",
                "Date to must be greater than or equal to date from.");

        public static readonly DomainError ParticipantHasPayments =
            new(DomainErrorType.Validation,
                "Trip.ParticipantHasPayments",
                "Participant has payments.");

        public static readonly DomainError NoPayments =
             new(DomainErrorType.Validation,
                 "Expense.HasNoPayments",
                 "Expense has no payments.");

        public static DomainError Validation<T>(string message) =>
            new(DomainErrorType.Validation,
                $"{typeof(T).Name}.Validation.Error",
                message);

    }
}
