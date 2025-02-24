using System.Text;
using LinkDotNet.StringBuilder;

namespace HjsonSharp;

/// <summary>
/// A reader that can read runes from a list of runes.
/// </summary>
public class ListRuneReader : RuneReader {
    /// <summary>
    /// The list to read runes from.
    /// </summary>
    public IList<Rune> InnerList { get; set; }
    /// <summary>
    /// The current index in <see cref="InnerList"/>.
    /// </summary>
    public int InnerListIndex { get; set; }

    /// <summary>
    /// Constructs a reader that reads runes from a list of runes.
    /// </summary>
    public ListRuneReader(IList<Rune> List) {
        InnerList = List;
    }

    /// <inheritdoc/>
    public override long Position {
        get => InnerListIndex;
        set => InnerListIndex = (int)value;
    }
    /// <inheritdoc/>
    public override long Length {
        get => InnerList.Count;
    }

    /// <summary>
    /// Reads a rune at the current index and advances the index.
    /// </summary>
    public override Rune? Read() {
        if (InnerListIndex >= InnerList.Count) {
            return null;
        }
        Rune Result = InnerList[InnerListIndex];
        InnerListIndex++;
        return Result;
    }
    /// <summary>
    /// Reads a rune at the current index without advancing the index.
    /// </summary>
    public override Rune? Peek() {
        if (InnerListIndex >= InnerList.Count) {
            return null;
        }
        Rune Result = InnerList[InnerListIndex];
        return Result;
    }
    /// <inheritdoc/>
    public override string ReadToEnd() {
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);
        for (; InnerListIndex < InnerList.Count; InnerListIndex++) {
            StringBuilder.Append(InnerList[InnerListIndex]);
        }
        InnerListIndex = InnerList.Count;
        return StringBuilder.ToString();
    }
}