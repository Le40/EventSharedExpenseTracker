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
        public static implicit operator DomainResult<T>(DomainError error)
            => Fail([error]);

    }
}
