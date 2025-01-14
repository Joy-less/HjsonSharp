﻿using System.Text;

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

    /// <summary>
    /// Reads a rune at the current index and advances the index.
    /// </summary>
    public override Rune? ReadRune() {
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
    public override Rune? PeekRune() {
        if (InnerListIndex >= InnerList.Count) {
            return null;
        }
        Rune Result = InnerList[InnerListIndex];
        return Result;
    }
}