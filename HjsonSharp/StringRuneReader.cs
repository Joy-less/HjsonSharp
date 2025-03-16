using System.Text;
using System.Buffers;

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
    /// The start index to read from <see cref="InnerString"/>.
    /// </summary>
    public int InnerStringOffset { get; set; }
    /// <summary>
    /// The maximum count to read from <see cref="InnerString"/>.
    /// </summary>
    public int InnerStringCount { get; set; }
    /// <summary>
    /// The current actual index in <see cref="InnerString"/>.
    /// </summary>
    public int InnerStringIndex { get; set; }

    /// <summary>
    /// Constructs a reader that reads runes from a string.
    /// </summary>
    public StringRuneReader(string String, int Index, int Count) {
        InnerString = String;
        InnerStringOffset = Index;
        InnerStringCount = Count;
        InnerStringIndex = Index;
    }
    /// <inheritdoc cref="StringRuneReader(string, int, int)"/>
    public StringRuneReader(string String)
        : this(String, 0, String.Length) {
    }

    /// <inheritdoc/>
    public override long Position {
        get => InnerStringIndex;
        set => InnerStringIndex = (int)value;
    }
    /// <inheritdoc/>
    public override long Length {
        get => InnerStringCount + InnerStringOffset;
    }

    /// <summary>
    /// Reads a rune at the current index and advances the index.
    /// </summary>
    public override Rune? Read() {
        if (InnerStringIndex >= InnerStringCount + InnerStringOffset) {
            return null;
        }
        if (Rune.DecodeFromUtf16(AsSpan(), out Rune Result, out int CharsConsumed) is not OperationStatus.Done) {
            throw new InvalidOperationException("Could not decode rune from string");
        }
        InnerStringIndex += CharsConsumed;
        return Result;
    }
    /// <summary>
    /// Reads a rune at the current index without advancing the index.
    /// </summary>
    public override Rune? Peek() {
        if (InnerStringIndex >= InnerStringCount + InnerStringOffset) {
            return null;
        }
        if (Rune.DecodeFromUtf16(AsSpan(), out Rune Result, out _) is not OperationStatus.Done) {
            throw new InvalidOperationException("Could not decode rune from string");
        }
        return Result;
    }
    /// <summary>
    /// Peeks the rune at the next index and checks if it matches the expected rune.
    /// </summary>
    public override bool TryRead(Rune? Expected) {
        if (InnerStringIndex >= InnerStringCount + InnerStringOffset) {
            return Expected is null;
        }
        if (Rune.DecodeFromUtf16(AsSpan(), out Rune Result, out int CharsConsumed) is not OperationStatus.Done) {
            throw new InvalidOperationException("Could not decode rune from string");
        }
        if (Result != Expected) {
            return false;
        }
        InnerStringIndex += CharsConsumed;
        return true;
    }
    /// <inheritdoc cref="TryRead(Rune?)"/>
    public override bool TryRead(char Expected) {
        if (InnerStringIndex >= InnerStringCount + InnerStringOffset) {
            return false;
        }
        if (InnerString[InnerStringIndex] != Expected) {
            return false;
        }
        InnerStringIndex++;
        return true;
    }
    /// <inheritdoc/>
    public override string ReadToEnd() {
        if (InnerStringIndex == 0) {
            return InnerString;
        }
        ReadOnlySpan<char> CharsRead = AsSpan();
        InnerStringIndex += CharsRead.Length;
        return CharsRead.ToString();
    }

    /// <summary>
    /// Returns a span of the remaining characters in the reader.
    /// </summary>
    public ReadOnlySpan<char> AsSpan() {
        return InnerString.AsSpan(InnerStringIndex..(InnerStringCount + InnerStringOffset));
    }
}