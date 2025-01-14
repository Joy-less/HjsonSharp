using System.Buffers;
using System.Text;

namespace HjsonSharp;

/// <summary>
/// A reader that can read runes from a byte stream according to a specified encoding.
/// </summary>
public class StreamRuneReader : RuneReader {
    /// <summary>
    /// The byte stream to decode runes from.
    /// </summary>
    public Stream InnerStream { get; set; }
    /// <summary>
    /// The text encoding to use when decoding runes from <see cref="InnerStream"/>.
    /// </summary>
    public Encoding? InnerStreamEncoding { get; set; }

    /// <summary>
    /// Constructs a reader that reads runes from a byte stream.
    /// </summary>
    /// <param name="Stream">Should be a <see cref="BufferedStream"/> or a <see cref="MemoryStream"/> for performance reasons.</param>
    public StreamRuneReader(Stream Stream, Encoding? Encoding) {
        InnerStream = Stream;
        InnerStreamEncoding = Encoding ?? DetectEncoding();
    }

    /// <inheritdoc/>
    public override long Position {
        get => InnerStream.Position;
        set => InnerStream.Position = value;
    }

    /// <summary>
    /// Decodes a rune from the stream according to the specified encoding.<br/>
    /// Supports <see cref="Encoding.UTF8"/>, <see cref="Encoding.Unicode"/>, <see cref="Encoding.BigEndianUnicode"/>,
    /// <see cref="Encoding.UTF32"/> and <see cref="Encoding.ASCII"/>.
    /// </summary>
    public override Rune? ReadRune() {
        long OriginalPosition = Position;
        try {
            // UTF-8
            if (InnerStreamEncoding == Encoding.UTF8) {
                // Read first byte
                int FirstByte = InnerStream.ReadByte();
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
                int TotalBytesRead = 1 + InnerStream.Read(Bytes[1..]);

                // Decode rune from UTF-8 bytes
                if (Rune.DecodeFromUtf8(Bytes[..TotalBytesRead], out Rune Result, out _) is not OperationStatus.Done) {
                    throw new HjsonException("Could not decode rune from UTF-8 bytes");
                }
                return Result;
            }
            // ASCII
            else if (InnerStreamEncoding == Encoding.ASCII) {
                // Read 1 byte
                int Byte = InnerStream.ReadByte();
                if (Byte < 0) {
                    return null;
                }

                // Ensure byte is valid ASCII character
                if (Byte > 127) {
                    throw new HjsonException("Could not decode rune from ASCII bytes");
                }
                return new Rune((byte)Byte);
            }
            // UTF-32
            else if (InnerStreamEncoding == Encoding.UTF32) {
                // Read 4 bytes
                Span<byte> Bytes = stackalloc byte[4];
                int BytesRead = InnerStream.Read(Bytes);
                if (BytesRead == 0) {
                    return null;
                }

                // Ensure 4 bytes were read
                if (BytesRead != 4) {
                    throw new HjsonException("Could not decode rune from UTF-32 bytes");
                }

                // Convert bytes to chars
                Span<char> Chars = stackalloc char[2];
                int CharsRead = InnerStreamEncoding.GetChars(Bytes, Chars);

                // Ensure 1 or 2 chars were read
                if (CharsRead == 1) {
                    return new Rune(Chars[0]);
                }
                else if (CharsRead == 2) {
                    return new Rune(Chars[0], Chars[1]);
                }
                else {
                    throw new HjsonException("Could not decode rune from UTF-32 bytes");
                }
            }
            // UTF-16
            else if (InnerStreamEncoding == Encoding.Unicode || InnerStreamEncoding == Encoding.BigEndianUnicode) {
                // Read 2 bytes
                Span<byte> Bytes = stackalloc byte[4];
                int BytesRead = InnerStream.Read(Bytes[..2]);
                if (BytesRead == 0) {
                    return null;
                }

                // Ensure 2 bytes were read
                if (BytesRead != 2) {
                    throw new HjsonException("Could not decode rune from UTF-16 bytes");
                }

                // If not in surrogate pair, convert char to rune
                if (GetUtf16SequenceLength(Bytes, InnerStreamEncoding == Encoding.BigEndianUnicode) == 2) {
                    // Convert bytes to char
                    Span<char> OneChars = stackalloc char[1];
                    int OneCharsRead = InnerStreamEncoding.GetChars(Bytes[..BytesRead], OneChars);

                    // Ensure 1 char was read
                    if (OneCharsRead != 1) {
                        throw new HjsonException("Could not decode rune from UTF-16 bytes");
                    }
                    return new Rune(OneChars[0]);
                }

                // Read 2 more bytes
                BytesRead += InnerStream.Read(Bytes[BytesRead..]);

                // Convert bytes to char
                Span<char> TwoChars = stackalloc char[2];
                int TwoCharsRead = InnerStreamEncoding.GetChars(Bytes, TwoChars);

                // Ensure 1 char was read
                if (TwoCharsRead != 2) {
                    throw new HjsonException("Could not decode rune from UTF-16 bytes");
                }

                // Convert surrogate pair to rune
                return new Rune(TwoChars[0], TwoChars[1]);
            }
            // Not supported
            else {
                throw new NotSupportedException($"Encoding not supported: `{InnerStreamEncoding}`");
            }
        }
        catch {
            // Reset position on exception
            Position = OriginalPosition;
            throw;
        }
    }
    /// <inheritdoc/>
    public override void Dispose() {
        base.Dispose();
        GC.SuppressFinalize(this);
        InnerStream.Dispose();
    }

    /// <summary>
    /// Decodes the preamble (Byte Order Mark / BOM) from the stream.<br/>
    /// If no preamble is found, <see cref="Encoding.UTF8"/> is assumed.<br/>
    /// Detects <see cref="Encoding.UTF8"/>, <see cref="Encoding.Unicode"/>, <see cref="Encoding.BigEndianUnicode"/> and <see cref="Encoding.UTF32"/>.
    /// </summary>
    /// <remarks>
    /// The stream should be at the beginning, and will be moved after the preamble.
    /// </remarks>
    public Encoding DetectEncoding() {
        long OriginalPosition = Position;
        int PreambleLength = 0;
        try {
            // Read up to 4 bytes
            Span<byte> LeadingBytes = stackalloc byte[4];
            int LeadingBytesRead = InnerStream.Read(LeadingBytes);
            ReadOnlySpan<byte> LeadingBytesReadOnly = LeadingBytes[..LeadingBytesRead];

            // UTF-8
            if (LeadingBytesReadOnly.StartsWith(Encoding.UTF8.Preamble)) {
                PreambleLength = Encoding.UTF8.Preamble.Length;
                return Encoding.UTF8;
            }
            // UTF-32
            else if (LeadingBytesReadOnly.StartsWith(Encoding.UTF32.Preamble)) {
                PreambleLength = Encoding.UTF32.Preamble.Length;
                return Encoding.UTF32;
            }
            // UTF-16
            else if (LeadingBytesReadOnly.StartsWith(Encoding.Unicode.Preamble)) {
                PreambleLength = Encoding.Unicode.Preamble.Length;
                return Encoding.Unicode;
            }
            // Big-endian UTF-16
            else if (LeadingBytesReadOnly.StartsWith(Encoding.BigEndianUnicode.Preamble)) {
                PreambleLength = Encoding.BigEndianUnicode.Preamble.Length;
                return Encoding.BigEndianUnicode;
            }
            // Fallback to UTF-8
            else {
                return Encoding.UTF8;
            }
        }
        finally {
            // Move to first byte after preamble
            Position = OriginalPosition + PreambleLength;
        }
    }

    /// <summary>
    /// Calculates the length in bytes of a single UTF-8 rune from the bits in its first byte.<br/>
    /// The result will be 1, 2, 3 or 4.
    /// </summary>
    public static int GetUtf8SequenceLength(byte FirstByte) {
        // https://codegolf.stackexchange.com/a/173577
        return (FirstByte - 160 >> 20 - FirstByte / 16) + 2;
    }
    /// <summary>
    /// Calculates the length in bytes of a single UTF-16 rune from the bits in its first two bytes.<br/>
    /// The result will be 2 or 4.
    /// </summary>
    public static int GetUtf16SequenceLength(ReadOnlySpan<byte> Bytes, bool IsBigEndian = false) {
        if (Bytes.Length < 2) {
            throw new ArgumentException("At least 2 bytes are required.", nameof(Bytes));
        }
        ushort Value = IsBigEndian
            ? (ushort)((Bytes[0] << 8) | Bytes[1])  // Big-endian: Most Significant Byte first
            : (ushort)((Bytes[1] << 8) | Bytes[0]); // Little-endian: Least Significant Byte first
        bool IsHighSurrogate = char.IsHighSurrogate((char)Value);
        return IsHighSurrogate ? 4 : 2;
    }
}