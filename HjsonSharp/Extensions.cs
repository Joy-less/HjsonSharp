using System.Text;

namespace HjsonSharp;

internal static class Extensions {
    /// <summary>
    /// Efficiently appends a <see cref="Rune"/> to a <see cref="StringBuilder"/>.<br/>
    /// See <see href="https://github.com/dotnet/runtime/discussions/111139"/>
    /// </summary>
    public static void AppendRune(this StringBuilder StringBuilder, Rune Rune) {
        Span<char> Chars = stackalloc char[2];
        int CharsWritten = Rune.EncodeToUtf16(Chars);
        StringBuilder.Append(Chars[..CharsWritten]);
    }
}