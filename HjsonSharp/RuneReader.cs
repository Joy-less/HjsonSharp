using System.Text;
using LinkDotNet.StringBuilder;

namespace HjsonSharp;

/// <summary>
/// A reader that can read a seekable sequence of runes.
/// </summary>
public abstract class RuneReader : IDisposable {
    /// <summary>
    /// The current position in the sequence.
    /// </summary>
    /// <remarks>
    /// This could refer to bytes, characters, or runes, depending on the implementation.
    /// </remarks>
    public abstract long Position { get; set; }
    /// <summary>
    /// The length of the sequence.
    /// </summary>
    /// <remarks>
    /// This could refer to bytes, characters, or runes, depending on the implementation.
    /// </remarks>
    public abstract long Length { get; }

    /// <summary>
    /// Reads the rune at the next position and advances the position by one rune.
    /// </summary>
    public abstract Rune? Read();

    /// <summary>
    /// Reads the rune at the next position without advancing the position.
    /// </summary>
    public virtual Rune? Peek() {
        long OriginalPosition = Position;
        try {
            return Read();
        }
        finally {
            Position = OriginalPosition;
        }
    }
    /// <summary>
    /// Peeks the rune at the next position and checks if it matches the expected rune.
    /// </summary>
    public virtual bool TryRead(Rune? Expected) {
        long OriginalPosition = Position;
        if (Read() != Expected) {
            Position = OriginalPosition;
            return false;
        }
        return true;
    }
    /// <inheritdoc cref="TryRead(Rune?)"/>
    public virtual bool TryRead(char Expected) {
        return TryRead(new Rune(Expected));
    }
    /// <summary>
    /// Reads every remaining rune in the reader and concatenates them to a string.
    /// </summary>
    public virtual string ReadToEnd() {
        ValueStringBuilder StringBuilder = new();
        while (Read() is Rune Rune) {
            StringBuilder.Append(Rune);
        }
        return StringBuilder.ToString();
    }
    /// <summary>
    /// Releases all unmanaged resources used by the reader.
    /// </summary>
    public virtual void Dispose() {
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// An exception that occurred when reading/decoding runes.
/// </summary>
public class RuneReaderException(string? Message = null) : Exception(Message);