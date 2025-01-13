using System.Buffers;
using System.Text;

namespace HjsonSharp;

/// <summary>
/// A stream that can read runes from a byte stream according to a specified encoding.
/// </summary>
/// <remarks>
/// <see cref="InnerStream"/> must be manually disposed.
/// </remarks>
public class RuneStream : Stream {
    /// <summary>
    /// The wrapped stream to decode runes from.
    /// </summary>
    public Stream InnerStream { get; }

    /// <summary>
    /// Constructs a stream that reads runes from a byte stream.<br/>
    /// The stream should be a <see cref="BufferedStream"/> if not stored in memory.
    /// </summary>
    public RuneStream(Stream InnerStream) {
        this.InnerStream = InnerStream;
    }

    #region Overrides
    /// <inheritdoc/>
    public override bool CanRead => InnerStream.CanRead;
    /// <inheritdoc/>
    public override bool CanSeek => InnerStream.CanSeek;
    /// <inheritdoc/>
    public override bool CanWrite => InnerStream.CanWrite;
    /// <inheritdoc/>
    public override long Length => InnerStream.Length;
    /// <inheritdoc/>
    public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }
    /// <inheritdoc/>
    public override bool CanTimeout => base.CanTimeout;
    /// <inheritdoc/>
    public override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }
    /// <inheritdoc/>
    public override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

    /// <inheritdoc/>
    public override void Flush() => InnerStream.Flush();
    /// <inheritdoc/>
    public override int Read(byte[] Buffer, int Offset, int Count) => InnerStream.Read(Buffer, Offset, Count);
    /// <inheritdoc/>
    public override int ReadByte() => InnerStream.ReadByte();
    /// <inheritdoc/>
    public override long Seek(long Offset, SeekOrigin Origin) => InnerStream.Seek(Offset, Origin);
    /// <inheritdoc/>
    public override void SetLength(long Value) => InnerStream.SetLength(Value);
    /// <inheritdoc/>
    public override void Write(byte[] Buffer, int Offset, int Count) => InnerStream.Write(Buffer, Offset, Count);
    /// <inheritdoc/>
    public override IAsyncResult BeginRead(byte[] Buffer, int Offset, int Count, AsyncCallback? Callback, object? State) => InnerStream.BeginRead(Buffer, Offset, Count, Callback, State);
    /// <inheritdoc/>
    public override IAsyncResult BeginWrite(byte[] Buffer, int Offset, int Count, AsyncCallback? Callback, object? State) => InnerStream.BeginWrite(Buffer, Offset, Count, Callback, State);
    /// <inheritdoc/>
    public override void Close() => InnerStream.Close();
    /// <inheritdoc/>
    public override void CopyTo(Stream Destination, int BufferSize) => InnerStream.CopyTo(Destination, BufferSize);
    /// <inheritdoc/>
    public override Task CopyToAsync(Stream Destination, int BufferSize, CancellationToken CancellationToken) => InnerStream.CopyToAsync(Destination, BufferSize, CancellationToken);
    /// <inheritdoc/>
    public override int EndRead(IAsyncResult AsyncResult) => InnerStream.EndRead(AsyncResult);
    /// <inheritdoc/>
    public override void EndWrite(IAsyncResult AsyncResult) => InnerStream.EndWrite(AsyncResult);
    /// <inheritdoc/>
    public override Task FlushAsync(CancellationToken CancellationToken) => InnerStream.FlushAsync(CancellationToken);
    /// <inheritdoc/>
    public override int Read(Span<byte> Buffer) => InnerStream.Read(Buffer);
    /// <inheritdoc/>
    public override Task<int> ReadAsync(byte[] Buffer, int Offset, int Count, CancellationToken CancellationToken) => InnerStream.ReadAsync(Buffer, Offset, Count, CancellationToken);
    /// <inheritdoc/>
    public override ValueTask<int> ReadAsync(Memory<byte> Buffer, CancellationToken CancellationToken = default) => InnerStream.ReadAsync(Buffer, CancellationToken);
    /// <inheritdoc/>
    public override void Write(ReadOnlySpan<byte> Buffer) => InnerStream.Write(Buffer);
    /// <inheritdoc/>
    public override Task WriteAsync(byte[] Buffer, int Offset, int Count, CancellationToken CancellationToken) => InnerStream.WriteAsync(Buffer, Offset, Count, CancellationToken);
    /// <inheritdoc/>
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> Buffer, CancellationToken CancellationToken = default) => InnerStream.WriteAsync(Buffer, CancellationToken);
    /// <inheritdoc/>
    public override void WriteByte(byte Value) => InnerStream.WriteByte(Value);
    #endregion

    /// <summary>
    /// Decodes a rune from the stream according to the specified encoding.<br/>
    /// Supports <see cref="Encoding.UTF8"/>, <see cref="Encoding.Unicode"/>, <see cref="Encoding.BigEndianUnicode"/>,
    /// <see cref="Encoding.UTF32"/> and <see cref="Encoding.ASCII"/>.
    /// </summary>
    public Rune? ReadRune(Encoding Encoding) {
        long OriginalPosition = Position;
        try {
            // UTF-8
            if (Encoding == Encoding.UTF8) {
                // Read first byte
                int FirstByte = ReadByte();
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
                int TotalBytesRead = 1 + Read(Bytes[1..]);

                // Decode rune from UTF-8 bytes
                if (Rune.DecodeFromUtf8(Bytes[..TotalBytesRead], out Rune Result, out _) is not OperationStatus.Done) {
                    throw new HjsonException("Could not decode rune from UTF-8 bytes");
                }
                return Result;
            }
            // ASCII
            else if (Encoding == Encoding.ASCII) {
                // Read 1 byte
                int Byte = ReadByte();
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
            else if (Encoding == Encoding.UTF32) {
                // Read 4 bytes
                Span<byte> Bytes = stackalloc byte[4];
                int BytesRead = Read(Bytes);

                // Ensure 4 bytes were read
                if (BytesRead != 4) {
                    throw new HjsonException("Could not decode rune from UTF-32 bytes");
                }

                // Convert bytes to chars
                Span<char> Chars = stackalloc char[2];
                int CharsRead = Encoding.GetChars(Bytes, Chars);

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
            else if (Encoding == Encoding.Unicode || Encoding == Encoding.BigEndianUnicode) {
                // Read 2 bytes
                Span<byte> Bytes = stackalloc byte[4];
                int BytesRead = Read(Bytes[..2]);

                // Ensure 2 bytes were read
                if (BytesRead != 2) {
                    throw new HjsonException("Could not decode rune from UTF-16 bytes");
                }

                // If not in surrogate pair, convert char to rune
                if (GetUtf16SequenceLength(Bytes, Encoding == Encoding.BigEndianUnicode) == 2) {
                    // Convert bytes to char
                    Span<char> OneChars = stackalloc char[1];
                    int OneCharsRead = Encoding.GetChars(Bytes[..BytesRead], OneChars);

                    // Ensure 1 char was read
                    if (OneCharsRead != 1) {
                        throw new HjsonException("Could not decode rune from UTF-16 bytes");
                    }
                    return new Rune(OneChars[0]);
                }

                // Read 2 more bytes
                BytesRead += Read(Bytes[BytesRead..]);

                // Convert bytes to char
                Span<char> TwoChars = stackalloc char[2];
                int TwoCharsRead = Encoding.GetChars(Bytes, TwoChars);

                // Ensure 1 char was read
                if (TwoCharsRead != 2) {
                    throw new HjsonException("Could not decode rune from UTF-16 bytes");
                }

                // Convert surrogate pair to rune
                return new Rune(TwoChars[0], TwoChars[1]);
            }
            // Not supported
            else {
                throw new NotSupportedException($"Encoding not supported: `{Encoding}`");
            }
        }
        catch {
            Position = OriginalPosition;
            throw;
        }
    }
    /// <inheritdoc cref="ReadRune(Encoding)"/>
    public Rune? PeekRune(Encoding Encoding) {
        long OriginalPosition = Position;
        try {
            return ReadRune(Encoding);
        }
        finally {
            Position = OriginalPosition;
        }
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
            int LeadingBytesRead = Read(LeadingBytes);
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