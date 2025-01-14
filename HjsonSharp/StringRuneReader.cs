using System.Buffers;
using System.Text;

namespace HjsonSharp;

/// <summary>
/// A reader that can read runes from a string.
/// </summary>
public class StringRuneReader : RuneReader {
    /// <summary>
    /// The string to read runes from.
    /// </summary>
    public string InnerString { get; set; }
    /// <summary>
    /// The current index in <see cref="InnerString"/>.
    /// </summary>
    public int InnerStringIndex { get; set; }

    /// <summary>
    /// Constructs a reader that reads runes from a string.
    /// </summary>
    public StringRuneReader(string String) {
        InnerString = String;
    }

    /// <inheritdoc/>
    public override long Position {
        get => InnerStringIndex;
        set => InnerStringIndex = (int)value;
    }

    /// <summary>
    /// Reads a rune at the current index and advances the index.
    /// </summary>
    public override Rune? ReadRune() {
        if (InnerStringIndex >= InnerString.Length) {
            return null;
        }
        if (Rune.DecodeFromUtf16(InnerString.AsSpan(InnerStringIndex), out Rune Result, out int CharsConsumed) is not OperationStatus.Done) {
            throw new InvalidOperationException("Could not read rune from string");
        }
        InnerStringIndex += CharsConsumed;
        return Result;
    }
    /// <summary>
    /// Reads a rune at the current index without advancing the index.
    /// </summary>
    public override Rune? PeekRune() {
        if (InnerStringIndex >= InnerString.Length) {
            return null;
        }
        if (Rune.DecodeFromUtf16(InnerString.AsSpan(InnerStringIndex), out Rune Result, out _) is not OperationStatus.Done) {
            throw new InvalidOperationException("Could not read rune from string");
        }
        return Result;
    }
}