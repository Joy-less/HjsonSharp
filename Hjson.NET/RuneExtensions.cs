using System.Buffers;
using System.Text;

namespace Hjson.NET;

/// <summary>
/// Contains methods to efficiently decode runes from byte streams of various encodings.
/// </summary>
internal static class RuneExtensions {
    
    
    /*/// <summary>
    /// Calculates the length in bytes of a single UTF-16 rune from the bits in its first two bytes.<br/>
    /// The result will be 2 or 4.
    /// </summary>
    public static int GetUtf16SequenceLength2(ReadOnlySpan<byte> FirstTwoBytes) {
        bool IsHighSurrogate = BitConverter.ToUInt16(FirstTwoBytes) is >= 0xD800 and <= 0xDBFF;
        return IsHighSurrogate ? 4 : 2;
    }*/
    
}