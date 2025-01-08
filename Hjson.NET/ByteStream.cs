namespace Hjson.NET;

/*/// <summary>
/// A buffered layer for a <see cref="Stream"/> designed to read/peek 1 byte at a time.<br/>
/// Based on <see href="https://stackoverflow.com/a/73572227"/>.
/// </summary>
public class ByteStream(Stream Stream, int BufferSize = 4096) : Stream {
    public readonly BufferedStream BufferedStream = new(Stream, BufferSize);

    /// <value>
    /// A <see cref="byte"/> or -1.
    /// </value>
    private int Buffer;
    /// <value>
    /// 0 or 1.
    /// </value>
    private int BufferLength;

    public sealed override long Position {
        get => BufferedStream.Position - BufferLength;
        set {
            BufferedStream.Position = value;
            BufferLength = 0;
        }
    }

    public int PeekByte() {
        if (BufferLength > 0) {
            return Buffer;
        }
        Buffer = BufferedStream.ReadByte();
        BufferLength = 1;
        return Buffer;
    }
    public sealed override int ReadByte() {
        if (BufferLength == 0) {
            return BufferedStream.ReadByte();
        }
        if (Buffer < 0) {
            return -1;
        }
        BufferLength = 0;
        return Buffer;
    }

    public sealed override long Seek(long Offset, SeekOrigin Origin) {
        long NewPosition = BufferedStream.Seek(Offset, Origin);
        BufferLength = 0;
        return NewPosition;
    }
    public sealed override int Read(byte[] OutputBuffer, int Offset, int Count) {
        if (Count == 0) {
            return 0;
        }
        if (BufferLength == 0) {
            return BufferedStream.Read(OutputBuffer, Offset, Count);
        }
        if (Buffer < 0) {
            return 0;
        }

        OutputBuffer[Offset] = (byte)Buffer;
        BufferLength = 0;
        if (Count == 1) {
            return Count;
        }

        int ReadByteCount = BufferedStream.Read(OutputBuffer, Offset + 1, Count - 1);
        return ReadByteCount + 1;
    }
    public sealed override long Length => BufferedStream.Length;
    public sealed override bool CanRead => BufferedStream.CanRead;
    public sealed override bool CanSeek => BufferedStream.CanSeek;
    public sealed override bool CanWrite => BufferedStream.CanWrite;
    public sealed override bool CanTimeout => BufferedStream.CanTimeout;
    public sealed override int ReadTimeout { get => BufferedStream.ReadTimeout; set => BufferedStream.ReadTimeout = value; }
    public sealed override int WriteTimeout { get => BufferedStream.WriteTimeout; set => BufferedStream.WriteTimeout = value; }
    public sealed override void Flush() => BufferedStream.Flush();
    public sealed override void SetLength(long Value) => BufferedStream.SetLength(Value);
    public sealed override void WriteByte(byte Value) => BufferedStream.WriteByte(Value);
    public sealed override void Write(byte[] InputBuffer, int Offset, int Count) => BufferedStream.Write(InputBuffer, Offset, Count);

    protected override void Dispose(bool Disposing) {
        if (Disposing) {
            BufferedStream.Dispose();
        }
        base.Dispose(Disposing);
    }
}*/