using System.Diagnostics.CodeAnalysis;

namespace Hjson.NET;

/*public readonly struct Result(Exception? FailureException) {
    private readonly Exception? FailureException = FailureException;

    /// <summary>
    /// If failure, returns the exception; otherwise, throws an exception.
    /// </summary>
    public Exception Exception => IsFailure ? FailureException! : throw new Exception("Result has no exception");
    /// <summary>
    /// If failure, returns the exception; otherwise, returns <see langword="null"/>.
    /// </summary>
    public Exception? ExceptionOrNull => FailureException;
    /// <summary>
    /// If there was no exception, returns <see langword="true"/>; otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool IsSuccess => FailureException is null;
    /// <summary>
    /// If there was an exception, returns <see langword="true"/>; otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool IsFailure => FailureException is not null;

    public bool TryGetException([NotNullWhen(true)] out Exception? Exception) {
        if (IsFailure) {
            Exception = FailureException!;
            return true;
        }
        else {
            Exception = null;
            return false;
        }
    }

    public static Result Success() => new(null);
    public static Result Failure(Exception Exception) => new(Exception);
    public static Result Failure(string Error) => new(new Exception(Error));

    public static implicit operator Result(Exception? Exception) => Exception is null ? Success() : Failure(Exception.Message);
}

public readonly struct Result<T>(T? SuccessValue, Exception? FailureException) {
    private readonly T? SuccessValue = SuccessValue;
    private readonly Exception? FailureException = FailureException;

    /// <summary>
    /// If success, returns the value; otherwise, throws the exception.
    /// </summary>
    public T Value => IsSuccess ? SuccessValue! : throw FailureException!;
    /// <summary>
    /// If success, returns the value; otherwise, returns the default value of <typeparamref name="T"/>.
    /// </summary>
    public T? ValueOrDefault => SuccessValue;
    /// <summary>
    /// If failure, returns the exception; otherwise, throws an exception.
    /// </summary>
    public Exception Exception => IsFailure ? FailureException! : throw new Exception("Result has no exception");
    /// <summary>
    /// If failure, returns the exception; otherwise, returns <see langword="null"/>.
    /// </summary>
    public Exception? ExceptionOrNull => FailureException;
    /// <summary>
    /// If there was no exception, returns <see langword="true"/>; otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool IsSuccess => FailureException is null;
    /// <summary>
    /// If there was an exception, returns <see langword="true"/>; otherwise, returns <see langword="false"/>.
    /// </summary>
    public bool IsFailure => FailureException is not null;

    public bool TryGetValue([NotNullWhen(true)] out T? Value) {
        if (IsSuccess) {
            Value = SuccessValue!;
            return true;
        }
        else {
            Value = default;
            return false;
        }
    }
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out Exception? Exception) {
        if (IsSuccess) {
            Value = SuccessValue!;
            Exception = null;
            return true;
        }
        else {
            Value = default;
            Exception = FailureException!;
            return false;
        }
    }
    public bool TryGetException([NotNullWhen(true)] out Exception? Exception) {
        if (IsFailure) {
            Exception = FailureException!;
            return true;
        }
        else {
            Exception = null;
            return false;
        }
    }
    public Result<TOut> SelectResult<TOut>(Func<T, TOut> Selector) {
        if (IsSuccess) {
            return Selector(SuccessValue!);
        }
        else {
            return FailureException!;
        }
    }

    public static Result<T> Success(T Value) => new(Value, null);
    public static Result<T> Failure(Exception Exception) => new(default, Exception);
    public static Result<T> Failure(string Error) => new(default, new Exception(Error));

    public static implicit operator Result<T>(T Value) => Success(Value);
    public static implicit operator Result<T>(Exception Exception) => Failure(Exception.Message);
}*/