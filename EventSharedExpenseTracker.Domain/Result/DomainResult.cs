namespace EventSharedExpenseTracker.Domain.Result
{
    public sealed class DomainResult
    {
        public bool IsSuccess => Errors.Count == 0;
        public IReadOnlyList<DomainError> Errors { get; }

        private DomainResult(IEnumerable<DomainError> errors) => 
            Errors = errors.ToList();

        public static DomainResult Ok() => 
            new([]);
        public static DomainResult Fail(IEnumerable<DomainError> errors) =>
            new(errors);

        public static implicit operator DomainResult(List<DomainError> errors) =>
            Fail(errors);

        public static implicit operator DomainResult(DomainError error) =>
            Fail([error]);
    }

    public sealed class DomainResult<T>
    {
        private readonly T? _value;
        public T Value =>
            IsSuccess
                ? _value!
                : throw new InvalidOperationException("Cannot access value of failed domain result.");
        public bool IsSuccess => Errors.Count == 0;
        public IReadOnlyList<DomainError> Errors { get; }

        private DomainResult(T value)
        {
            _value = value;
            Errors = [];
        }

        private DomainResult(IEnumerable<DomainError> errors)
        {
            _value = default;
            Errors = errors.ToList();
        }

        public static DomainResult<T> Ok(T value)
            => new(value);
        public static DomainResult<T> Fail(IEnumerable<DomainError> errors)
            => new(errors);

        public static implicit operator DomainResult<T>(T value)
            => Ok(value);

        public static implicit operator DomainResult<T>(List<DomainError> errors)
            => Fail(errors);

    }

    public sealed record DomainError(
         string Code,
         string Message,
         DomainErrorType Type);

    public enum DomainErrorType
    {
        Validation,
        Conflict,
        NotFound
    }

    public static class DomainErrors
    {
        public static readonly DomainError ParticipantNameRequired =
            new("Trip.ParticipantNameRequired",
                "Participant name is required.",
                 DomainErrorType.Validation);

        public static readonly DomainError ParticipantAlreadyExists =
            new("Trip.ParticipantAlreadyExists",
                "Participant already exists.",
                DomainErrorType.Conflict);

        public static readonly DomainError InvalidTripDateRange =
            new("Trip.InvalidDateRange", 
                "Date to must be greater than or equal to date from.",
                DomainErrorType.Validation);

        public static DomainError NotFound<T>() =>
            new($"{typeof(T).Name}.NotFound",
                $"{typeof(T).Name} not found.",
                DomainErrorType.NotFound);

        public static readonly DomainError ParticipantHasPayments =
            new("Trip.ParticipantHasPayments", 
                "Participant has payments.",
                DomainErrorType.Validation);

        public static DomainError Validation<T>(string message) =>
            new($"{typeof(T).Name}.Validation.Error",
                message,
                DomainErrorType.Validation);

    }
}
