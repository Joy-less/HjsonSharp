using System.Buffers;
using System.Text;

namespace Hjson.NET;

/// <summary>
/// Contains methods to efficiently decode runes from byte streams of various encodings.
/// </summary>
internal static class RuneExtensions {
    /// <summary>
    /// Decodes a rune from the beginning of the stream according to the specified encoding.<br/>
    /// Supports <see cref="UTF8Encoding"/>, <see cref="UnicodeEncoding"/>, <see cref="UTF32Encoding"/> and <see cref="ASCIIEncoding"/>.<br/>
    /// The stream should be a <see cref="BufferedStream"/> for performance.
    /// </summary>
    public static Rune? GetRuneFromStream(this Stream Stream, Encoding Encoding) {
        // UTF-8
        if (Encoding is UTF8Encoding) {
            // Read first byte
            int FirstByte = Stream.ReadByte();
            if (FirstByte < 0) {
                return null;
            }

            // Single byte character performance optimisation
            if (FirstByte <= 127) {
                return new Rune((byte)FirstByte);
            }

            // Get number of bytes in UTF8 character
            int SequenceLength = GetUtf8SequenceLength((byte)FirstByte);

            // Read remaining bytes (up to 3 more)
            Span<byte> Bytes = stackalloc byte[SequenceLength];
            Bytes[0] = (byte)FirstByte;
            int TotalBytes = 1 + Stream.Read(Bytes[1..]);

            // Decode rune from UTF-8 bytes
            if (Rune.DecodeFromUtf8(Bytes, out Rune Result, out _) is not OperationStatus.Done) {
                throw new HjsonException("Could not decode rune from UTF-8 bytes");
            }
            return Result;
        }
        // ASCII
        else if (Encoding is ASCIIEncoding) {
            // Read 1 byte
            int Byte = Stream.ReadByte();
            if (Byte < 0) {
                return null;
            }

            // Ensure byte is valid ASCII character
            if (Byte is < 0 or > 127) {
                throw new HjsonException("Could not decode rune from ASCII bytes");
            }
            return new Rune((byte)Byte);
        }
        // UTF-32
        else if (Encoding is UTF32Encoding) {
            // Read 4 bytes
            Span<byte> Bytes = stackalloc byte[4];
            int BytesRead = Stream.Read(Bytes);

            // Ensure 4 bytes were read
            if (BytesRead != 4) {
                throw new HjsonException("Could not decode rune from UTF-32 bytes");
            }

            // Convert bytes to chars
            Span<char> Chars = stackalloc char[2];
            int CharsRead = Encoding.GetChars(Bytes, Chars);

            // Ensure 2 chars were read
            if (CharsRead != 2) {
                throw new HjsonException("Could not decode rune from UTF-32 bytes");
            }
            return new Rune(Chars[0], Chars[1]);
        }
        // UTF-16
        else if (Encoding is UnicodeEncoding) {
            // Read up to 4 bytes
            Span<byte> RuneBytes = stackalloc byte[4];
            int BytesRead = Stream.Read(RuneBytes);

            // Decode rune from UTF-16 bytes
            Span<char> Chars = stackalloc char[2];
            int CharCount = Encoding.GetChars(RuneBytes, Chars);
            int BytesConsumed = CharCount * sizeof(char);
            if (Rune.DecodeFromUtf16(Chars[..CharCount], out Rune Result, out _) is not OperationStatus.Done) {
                throw new HjsonException("Could not decode rune from UTF-16 bytes");
            }

            // Backtrack unused bytes
            int BytesRemaining = BytesRead - BytesConsumed;
            if (BytesRemaining > 0) {
                Stream.Position -= BytesRemaining;
            }
            return Result;
        }
        // Not supported
        else {
            throw new NotSupportedException($"Encoding not supported: `{Encoding}`");
        }
    }
    /// <summary>
    /// Calculates the length of a single UTF8 character from the bits in its first byte.<br/>
    /// See <see href="https://codegolf.stackexchange.com/a/173577"/>
    /// </summary>
    public static int GetUtf8SequenceLength(byte FirstByte) {
        return (FirstByte - 160 >> 20 - FirstByte / 16) + 2;
    }
}