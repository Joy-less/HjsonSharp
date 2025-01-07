using System.Text;

namespace HjsonSharp;

internal static class Extensions {
    public static void AppendRune(this StringBuilder StringBuilder, Rune Rune) {
        Span<char> Chars = stackalloc char[2];
        int CharsWritten = Rune.EncodeToUtf16(Chars);
        StringBuilder.Append(Chars[..CharsWritten]);
    }
}