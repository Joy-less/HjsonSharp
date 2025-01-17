using System.Diagnostics.CodeAnalysis;

namespace HjsonSharp;

/// <summary>
/// Success or an error.
/// </summary>
public interface IHjsonResult {
    /// <summary>
    /// Returns the error that occurred or <see langword="default"/>(<see cref="HjsonError"/>).
    /// </summary>
    public HjsonError ErrorOrDefault { get; }
    /// <summary>
    /// Returns the error that occurred or throws an exception.
    /// </summary>
    public HjsonError Error { get; }
    /// <summary>
    /// Returns <see langword="true"/> if an error occurred.
    /// </summary>
    public bool IsError { get; }
    /// <summary>
    /// Returns <see langword="true"/> if an error occurred and provides the error or <see langword="default"/>(<see cref="HjsonError"/>).
    /// </summary>
    public bool TryGetError([NotNullWhen(true)] out HjsonError Error);
}

/// <summary>
/// A value or an error.
/// </summary>
public interface IHjsonResult<T> : IHjsonResult {
    /// <summary>
    /// Returns the value or <see langword="default"/>(<typeparamref name="T"/>).
    /// </summary>
    public T? ValueOrDefault { get; }
    /// <summary>
    /// Returns the value or throws an exception.
    /// </summary>
    public T Value { get; }
    /// <summary>
    /// Returns <see langword="true"/> if a value was successfully returned.
    /// </summary>
    public bool IsValue { get; }
    /// <summary>
    /// Transforms the value using a mapping function if a value was successfully returned or returns the error.
    /// </summary>
    public IHjsonResult<TNew> Try<TNew>(Func<T, TNew> Map);
    /// <summary>
    /// Returns <see langword="true"/> if a value was successfully returned and provides the value or <see langword="default"/>(<typeparamref name="T"/>)
    /// and the error or <see langword="default"/>(<see cref="HjsonError"/>).
    /// </summary>
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out HjsonError Error);
    /// <summary>
    /// Returns <see langword="true"/> if a value was successfully returned and provides the value or <see langword="default"/>(<typeparamref name="T"/>).
    /// </summary>
    public bool TryGetValue([NotNullWhen(true)] out T? Value);
    /// <summary>
    /// Returns <see langword="true"/> if a value was successfully returned and is equal to <paramref name="Other"/>.
    /// </summary>
    public bool ValueEquals(T? Other);
}

/// <summary>
/// Success or an error.
/// </summary>
public readonly struct HjsonResult : IHjsonResult {
    /// <summary>
    /// A successful result.
    /// </summary>
    public static HjsonResult Success { get; } = new();

    /// <inheritdoc/>
    public HjsonError ErrorOrDefault { get; }
    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(ErrorOrDefault))]
    public bool IsError { get; }

    /// <summary>
    /// Constructs a successful result.
    /// </summary>
    public HjsonResult() {
        IsError = false;
    }
    /// <summary>
    /// Constructs a failed result.
    /// </summary>
    public HjsonResult(HjsonError Error) {
        ErrorOrDefault = Error;
        IsError = true;
    }
    /// <summary>
    /// Constructs a failed result.
    /// </summary>
    public HjsonResult(string? ErrorMessage)
        : this(new HjsonError(ErrorMessage)) {
    }

    /// <summary>
    /// Returns the error that occurred or throws an exception.
    /// </summary>
    public HjsonError Error => IsError ? ErrorOrDefault : throw new InvalidOperationException("Result was value");

    /// <summary>
    /// Returns a string representation of the result.
    /// </summary>
    public override string ToString() {
        if (IsError) {
            return $"Error: {ErrorOrDefault.Message}";
        }
        else {
            return "Success";
        }
    }
    /// <inheritdoc/>
    public bool TryGetError([NotNullWhen(false)] out HjsonError Error) {
        Error = ErrorOrDefault;
        return IsError;
    }

    /// <summary>
    /// Creates a successful result or a failed result an error.
    /// </summary>
    public static implicit operator HjsonResult(HjsonError? Error) {
        return Error is not null ? new HjsonResult(Error.Value) : Success;
    }
}

/// <summary>
/// A value or an error from parsing HJSON.
/// </summary>
public readonly struct HjsonResult<T> : IHjsonResult, IHjsonResult<T> {
    /// <inheritdoc/>
    public T? ValueOrDefault { get; }
    /// <inheritdoc/>
    public HjsonError ErrorOrDefault { get; }
    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(ErrorOrDefault))]
    public bool IsError { get; }

    /// <summary>
    /// Constructs a successful result.
    /// </summary>
    public HjsonResult(T Value) {
        ValueOrDefault = Value;
        IsError = false;
    }
    /// <summary>
    /// Constructs a failed result.
    /// </summary>
    public HjsonResult(HjsonError Error) {
        ErrorOrDefault = Error;
        IsError = true;
    }
    /// <summary>
    /// Constructs a failed result.
    /// </summary>
    public HjsonResult(string? ErrorMessage)
        : this(new HjsonError(ErrorMessage)) {
    }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(ValueOrDefault))]
    public bool IsValue => !IsError;
    /// <inheritdoc/>
    public T Value => IsValue ? ValueOrDefault : throw new InvalidOperationException($"Result was error: \"{Error.Message}\"");
    /// <inheritdoc/>
    public HjsonError Error => IsError ? ErrorOrDefault : throw new InvalidOperationException("Result was value");

    /// <inheritdoc/>
    public override string ToString() {
        if (IsError) {
            return $"Error: {Error.Message}";
        }
        else {
            return $"Success: {Value}";
        }
    }
    /// <inheritdoc/>
    public HjsonResult<TNew> Try<TNew>(Func<T, TNew> Map) {
        return IsValue ? Map(Value) : Error;
    }
    /// <inheritdoc/>
    public bool TryGetError([NotNullWhen(false)] out HjsonError Error) {
        Error = ErrorOrDefault;
        return IsError;
    }
    /// <inheritdoc/>
    public bool TryGetValue([NotNullWhen(true)] out T? Value, [NotNullWhen(false)] out HjsonError Error) {
        Value = ValueOrDefault;
        Error = ErrorOrDefault;
        return IsValue;
    }
    /// <inheritdoc/>
    public bool TryGetValue([NotNullWhen(true)] out T? Value) {
        Value = ValueOrDefault;
        return IsValue;
    }
    /// <inheritdoc/>
    public bool ValueEquals(T? Other) {
        return IsValue && Equals(Value, Other);
    }

    /// <summary>
    /// Creates a successful result from a value.
    /// </summary>
    public static implicit operator HjsonResult<T>(T Value) {
        return new HjsonResult<T>(Value);
    }
    /// <summary>
    /// Creates a failed result from an error.
    /// </summary>
    public static implicit operator HjsonResult<T>(HjsonError Error) {
        return new HjsonResult<T>(Error);
    }

    IHjsonResult<TNew> IHjsonResult<T>.Try<TNew>(Func<T, TNew> Map) => Try(Map);
}

/// <summary>
/// An error that occurred when reading or writing HJSON.
/// </summary>
public readonly struct HjsonError {
    /// <summary>
    /// The error message for debugging purposes.
    /// </summary>
    public readonly string? Message { get; }
    /// <summary>
    /// Optional metadata such as an error code.
    /// </summary>
    public readonly object? Metadata { get; }

    /// <summary>
    /// Constructs an error that occurred when reading or writing HJSON.
    /// </summary>
    public HjsonError(string? Message, object? Metadata = null) {
        this.Message = Message;
        this.Metadata = Metadata;
    }

    /// <summary>
    /// Returns a string representation of the error.
    /// </summary>
    public override string ToString() {
        return $"Error: \"{Message}\""
            + (Metadata is not null ? $" (Metadata: {Metadata})" : "");
    }
}