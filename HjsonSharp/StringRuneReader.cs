using System.Text;
using System.Buffers;
using LinkDotNet.StringBuilder;

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
    /// The maximum count to read from <see cref="InnerString"/> offset by <see cref="InnerStringOffset"/>.
    /// </summary>
    public int InnerStringCount { get; set; }
    /// <summary>
    /// The current index in <see cref="InnerString"/>.
    /// </summary>
    public int InnerStringIndex { get; set; }

    /// <summary>
    /// Constructs a reader that reads runes from a string.
    /// </summary>
    public StringRuneReader(string String, int Index, int Count) {
        InnerString = String;
        InnerStringOffset = Index;
        InnerStringCount = Count;
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
            throw new InvalidOperationException("Could not read rune from string");
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
            throw new InvalidOperationException("Could not read rune from string");
        }
        return Result;
    }
    /// <inheritdoc/>
    public override string ReadToEnd() {
        using ValueStringBuilder StringBuilder = new();
        for (; InnerStringIndex < InnerStringCount + InnerStringOffset; InnerStringIndex++) {
            StringBuilder.Append(InnerString[InnerStringIndex]);
        }
        return StringBuilder.ToString();
    }

    /// <summary>
    /// Returns a span of the remaining characters in the reader.
    /// </summary>
    public ReadOnlySpan<char> AsSpan() {
        return InnerString.AsSpan(InnerStringIndex, InnerStringCount + InnerStringOffset - InnerStringIndex);
    }
}