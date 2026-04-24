namespace Relay.SharedKernel.Application;

/// <summary>
/// Outcome of an operation. Wraps success/failure without throwing for expected business errors.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public AppError Error { get; }

    protected Result(bool isSuccess, AppError error)
    {
        if (isSuccess && error != AppError.None)
        {
            throw new InvalidOperationException("A successful result cannot carry an error.");
        }
        if (!isSuccess && error == AppError.None)
        {
            throw new InvalidOperationException("A failed result must carry an error.");
        }
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, AppError.None);
    public static Result Failure(AppError error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, AppError.None);
    public static Result<TValue> Failure<TValue>(AppError error) => new(default, false, error);
}

/// <summary>
/// Typed result carrying a success payload.
/// </summary>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    internal Result(TValue? value, bool isSuccess, AppError error) : base(isSuccess, error)
    {
        _value = value;
    }

    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");
}

/// <summary>
/// A structured error. Code identifies the kind; Description is human-readable.
/// </summary>
public sealed record AppError(string Code, string Description)
{
    public static readonly AppError None = new(string.Empty, string.Empty);
    public static readonly AppError NullValue = new("Error.NullValue", "The provided value was null.");
}
