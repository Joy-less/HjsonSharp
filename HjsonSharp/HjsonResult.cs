using System.Diagnostics.CodeAnalysis;

namespace HjsonSharp;

public interface IHjsonResult {
    public HjsonError ErrorOrDefault { get; }
    public HjsonError Error { get; }
    public bool IsError { get; }
    public bool TryGetError([NotNullWhen(true)] out HjsonError Error);
}

public interface IHjsonResult<T> : IHjsonResult {
    public T? ValueOrDefault { get; }
    public T Value { get; }
    public bool IsValue { get; }
    public IHjsonResult<TNew> Try<TNew>(Func<T, TNew> Map);
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out HjsonError Error);
    public bool TryGetValue([NotNullWhen(true)] out T? Value);
    public bool ValueEquals(T? Other);
}

public readonly struct HjsonResult : IHjsonResult {
    public HjsonError ErrorOrDefault { get; }
    [MemberNotNullWhen(true, nameof(ErrorOrDefault))]
    public bool IsError { get; }

    public HjsonResult() {
        IsError = false;
    }
    public HjsonResult(HjsonError Error) {
        ErrorOrDefault = Error;
        IsError = true;
    }
    public HjsonResult(string? Error)
        : this(new HjsonError(Error)) {
    }

    public HjsonError Error => IsError ? ErrorOrDefault : throw new InvalidOperationException("Result was value");

    public override string ToString() {
        if (IsError) {
            return $"Error: {ErrorOrDefault.Message}";
        }
        else {
            return "Success";
        }
    }
    public bool TryGetError([NotNullWhen(false)] out HjsonError Error) {
        Error = ErrorOrDefault;
        return IsError;
    }

    public static implicit operator HjsonResult(HjsonError Error) {
        return new HjsonResult(Error);
    }

    public static HjsonResult Success { get; } = new();
}

public readonly struct HjsonResult<T> : IHjsonResult, IHjsonResult<T> {
    public T? ValueOrDefault { get; }
    public HjsonError ErrorOrDefault { get; }
    [MemberNotNullWhen(true, nameof(ErrorOrDefault))]
    public bool IsError { get; }

    public HjsonResult(T Value) {
        ValueOrDefault = Value;
        IsError = false;
    }
    public HjsonResult(HjsonError Error) {
        ErrorOrDefault = Error;
        IsError = true;
    }
    public HjsonResult(string? Error)
        : this(new HjsonError(Error)) {
    }

    [MemberNotNullWhen(true, nameof(ValueOrDefault))]
    public bool IsValue => !IsError;
    public T Value => IsValue ? ValueOrDefault : throw new InvalidOperationException($"Result was error: \"{Error.Message}\"");
    public HjsonError Error => IsError ? ErrorOrDefault : throw new InvalidOperationException("Result was value");

    public override string ToString() {
        if (IsError) {
            return $"Error: {Error.Message}";
        }
        else {
            return $"Success: {Value}";
        }
    }
    public HjsonResult<TNew> Try<TNew>(Func<T, TNew> Map) {
        return IsValue ? Map(Value) : Error;
    }
    public bool TryGetError([NotNullWhen(false)] out HjsonError Error) {
        Error = ErrorOrDefault;
        return IsError;
    }
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out HjsonError Error) {
        Value = ValueOrDefault;
        Error = ErrorOrDefault;
        return IsValue;
    }
    public bool TryGetValue([NotNullWhen(true)] out T? Value) {
        Value = ValueOrDefault;
        return IsValue;
    }
    public bool ValueEquals(T? Other) {
        return IsValue && Equals(Value, Other);
    }

    public static implicit operator HjsonResult<T>(T Value) {
        return new HjsonResult<T>(Value);
    }
    public static implicit operator HjsonResult<T>(HjsonError Error) {
        return new HjsonResult<T>(Error);
    }

    IHjsonResult<TNew> IHjsonResult<T>.Try<TNew>(Func<T, TNew> Map) => Try(Map);
}

/// <summary>
/// An error occurred when reading or writing HJSON.
/// </summary>
public readonly struct HjsonError {
    public readonly string? Message { get; }
    public readonly object? Metadata { get; }

    public HjsonError(string? Message, object? Metadata = null) {
        this.Message = Message;
        this.Metadata = Metadata;
    }

    public override string ToString() {
        return $"Error: \"{Message}\""
            + (Metadata is not null ? $" (Metadata: {Metadata})" : "");
    }
}

public static class ResultExtensions {
    public static IEnumerable<T> AsValues<T>(this IEnumerable<HjsonResult<T>> Results) {
        return Results.Select(Result => Result.Value);
    }
    public static IEnumerable<HjsonError> AsErrors(this IEnumerable<IHjsonResult> Results) {
        return Results.Select(Result => Result.Error);
    }
    public static IEnumerable<T> FilterValues<T>(this IEnumerable<HjsonResult<T>> Results) {
        return Results.Where(Result => Result.IsValue).AsValues();
    }
    public static IEnumerable<HjsonError> FilterErrors(this IEnumerable<IHjsonResult> Results) {
        return Results.Where(Result => Result.IsError).AsErrors();
    }
}