using System.Diagnostics.CodeAnalysis;

namespace HjsonSharp;

public interface IHjsonResult {
    public HjsonError? ErrorOrNull { get; }
    public HjsonError Error { get; }
    public bool IsError { get; }
}

public readonly struct HjsonResult : IHjsonResult {
    public HjsonError? ErrorOrNull { get; }

    [MemberNotNullWhen(true, nameof(ErrorOrNull))]
    public bool IsError { get; }

    public HjsonResult() {
        IsError = false;
    }
    public HjsonResult(HjsonError Error) {
        ErrorOrNull = Error;
        IsError = true;
    }
    public HjsonResult(string? Error)
        : this(new HjsonError(Error)) {
    }

    public HjsonError Error => IsError ? ErrorOrNull.Value : throw new InvalidOperationException("Result was value");

    public override string ToString() {
        if (IsError) {
            return $"Error: {Error.Message}";
        }
        else {
            return "Success";
        }
    }

    public static implicit operator HjsonResult(HjsonError Error) {
        return new HjsonResult(Error);
    }

    public static HjsonResult Success { get; } = new();
}

public readonly struct HjsonResult<T> : IHjsonResult {
    public T? ValueOrNull { get; }
    public HjsonError? ErrorOrNull { get; }

    [MemberNotNullWhen(true, nameof(ValueOrNull))]
    public bool IsValue { get; }
    [MemberNotNullWhen(true, nameof(ErrorOrNull))]
    public bool IsError { get; }

    public HjsonResult(T Value) {
        ValueOrNull = Value;
        IsValue = true;
        IsError = false;
    }
    public HjsonResult(HjsonError Error) {
        ErrorOrNull = Error;
        IsValue = false;
        IsError = true;
    }
    public HjsonResult(string? Error)
        : this(new HjsonError(Error)) {
    }

    public T Value => IsValue ? ValueOrNull : throw new InvalidOperationException($"Result was error: \"{Error.Message}\"");
    public HjsonError Error => IsError ? ErrorOrNull.Value : throw new InvalidOperationException("Result was value");

    public override string ToString() {
        if (IsError) {
            return $"Error: {Error.Message}";
        }
        else {
            return $"Success: {Value}";
        }
    }
    public HjsonResult<T2> Try<T2>(Func<T, T2> Map) {
        return IsValue ? Map(Value) : Error;
    }
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out HjsonError? Error) {
        Value = ValueOrNull;
        Error = ErrorOrNull;
        return IsValue;
    }
    public bool TryGetValue([NotNullWhen(true)] out T? Value) {
        return TryGetValue(out Value, out _);
    }

    public static implicit operator HjsonResult<T>(T Value) {
        return new HjsonResult<T>(Value);
    }
    public static implicit operator HjsonResult<T>(HjsonError Error) {
        return new HjsonResult<T>(Error);
    }
}

/// <summary>
/// An error occurred when reading or writing HJSON.
/// </summary>
public readonly struct HjsonError {
    public readonly string? Message { get; }

    public HjsonError(string? Message) {
        this.Message = Message;
    }

    public override string ToString() {
        return $"Error: {Message}";
    }
}