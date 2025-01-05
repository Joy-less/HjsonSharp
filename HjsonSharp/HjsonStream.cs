using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace HjsonSharp;

/// <summary>
/// A buffered layer for a <see cref="Stream"/> that can read and write UTF8-encoded JSON matching the <see href="https://json.org"/> specification.
/// </summary>
/// <remarks>
/// Supports parsing the following extra syntax:
/// <list type="bullet">
///   <item>Trailing commas (<c>,</c>)</item>
///   <item>Line breaks in strings (<c>\n</c>, <c>\r</c>)</item>
///   <item>Numbers starting with zero (<c>0</c>)</item>
///   <item>Numbers starting with a positive sign (<c>+</c>)</item>
/// </list>
/// </remarks>
public class HjsonStream(Stream Stream, int BufferSize = 4096) : ByteStream(Stream, BufferSize) {
    public StringBuilder? StringBuilder = new();

    public HjsonStream(string String) : this(new MemoryStream(Encoding.UTF8.GetBytes(String))) {
    }
    public T DeserializeValue<T>(int ValueLength) {
        // Create buffer for bytes in range
        byte[] Buffer = ArrayPool<byte>.Shared.Rent(ValueLength);
        try {
            // Read all bytes in range
            ReadExactly(Buffer, 0, ValueLength);

            // Deserialize element
            return JsonSerializer.Deserialize<T>(Buffer.AsSpan(0, ValueLength))!;
        }
        finally {
            // Return buffer
            ArrayPool<byte>.Shared.Return(Buffer);
        }
    }
    public JsonElement DeserializeValue(int ValueLength) {
        return DeserializeValue<JsonElement>(ValueLength);
    }
    public T DeserializeValueAt<T>(long StartPosition, long EndPosition) {
        // Get length of value
        int ValueLength = (int)(EndPosition - StartPosition + 1);

        // Seek value start position
        long OriginalPosition = Position;
        Position = StartPosition;

        try {
            // Deserialize value
            return DeserializeValue<T>(ValueLength);
        }
        finally {
            // Seek original position
            Position = OriginalPosition;
        }
    }
    public JsonElement DeserializeValueAt(long StartPosition, long EndPosition) {
        return DeserializeValueAt<JsonElement>(StartPosition, EndPosition);
    }
    public IEnumerable<JsonStreamMemberRef> FindMembers(IList<JsonStreamKey> TargetPath, Func<JsonStreamKey, bool>? KeyPredicate) {
        List<JsonStreamKey> Path = [];
        (long Position, JsonStreamKey Key)? CurrentMember = null;

        bool IsDirectChildOfTargetPath() {
            // Ensure path depth is one more than target path depth
            if (Path.Count != TargetPath.Count + 1) {
                return false;
            }
            // Ensure path is inside target path
            for (int Index = 0; Index < TargetPath.Count; Index++) {
                if (Path[Index] != TargetPath[Index]) {
                    return false;
                }
            }
            return true;
        }
        bool IsAtTargetPath() {
            return Path.SequenceEqual(TargetPath);
        }

        foreach (JsonStreamToken Token in ReadValue(Path)) {
            if (Token.Type is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                // Ensure child of target path
                if (!IsDirectChildOfTargetPath()) {
                    continue;
                }

                // Found a member (name/index) in target path
                JsonStreamKey Key = Path[^1];
                // Ensure member key matches predicate
                if (KeyPredicate is not null && !KeyPredicate(Key)) {
                    continue;
                }

                // Start member
                CurrentMember = (Token.Position, Key);
            }
            else if (Token.Type is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                // Skip tokens after target path
                if (IsAtTargetPath()) {
                    break;
                }

                // Ensure child of target path
                if (!IsDirectChildOfTargetPath()) {
                    continue;
                }
                // Ensure token ends a started member
                if (CurrentMember is null) {
                    continue;
                }

                // Output member reference
                yield return new JsonStreamMemberRef(CurrentMember.Value.Key, CurrentMember.Value.Position, Token.Position);

                // End member
                CurrentMember = null;
            }
            else if (Token.Type is JsonTokenType.PropertyName or JsonTokenType.Comment) {
                // Ignore
                continue;
            }
            else if (Token.Type is JsonTokenType.String or JsonTokenType.Number or JsonTokenType.True or JsonTokenType.False or JsonTokenType.Null) {
                // Ensure child of target path
                if (!IsDirectChildOfTargetPath()) {
                    continue;
                }

                // Found a member (name/index) in target path
                JsonStreamKey Key = Path[^1];
                // Ensure member key matches predicate
                if (KeyPredicate is not null && !KeyPredicate(Key)) {
                    continue;
                }

                // Output member reference
                yield return new JsonStreamMemberRef(Key, Token.Position, Position);
            }
            else {
                // Not implemented
                throw new NotImplementedException($"Token not handled: '{Token.Type}'");
            }
        }
    }
    public bool InsertMember(IList<JsonStreamKey> TargetPath, long MemberIndex, JsonStreamMember Member) {
        // Ensure member index is positive
        if (MemberIndex < 0) {
            throw new ArgumentException("Member index must be positive (pass 0 to insert at beginning)");
        }

        // Serialize member to insert
        byte[] SerializedMember = Member.SerializeToUtf8Bytes(AppendComma: true);

        // Find target index
        long CurrentIndex = 0;
        foreach (JsonStreamMemberRef CurrentMember in FindMembers(TargetPath, null)) {
            // Found target index
            if (CurrentIndex == MemberIndex) {
                Write(SerializedMember);
                return true;
            }
            // Next index
            CurrentIndex++;
        }

        // Target index not found in target path; append to the end of the collection
        if (PeekByte() is '}' or ']') {
            Write(SerializedMember);
            return true;
        }

        // Target path not found
        return false;
    }
    public bool InsertProperty(IList<JsonStreamKey> TargetPath, long PropertyIndex, string PropertyName, JsonElement Element) {
        return InsertMember(TargetPath, PropertyIndex, new JsonStreamMember(PropertyName, Element));
    }
    public bool InsertItem(IList<JsonStreamKey> TargetPath, long ItemIndex, JsonElement Element) {
        return InsertMember(TargetPath, ItemIndex, new JsonStreamMember(ItemIndex, Element));
    }
    public long DeleteMembers(IList<JsonStreamKey> TargetPath, Func<JsonStreamKey, bool>? KeyPredicate, Func<JsonStreamMember, bool>? Predicate, long MaxDeletions = long.MaxValue) {
        long CurrentDeletions = 0;
        foreach (JsonStreamMemberRef Member in FindMembers(TargetPath, KeyPredicate)) {
            // Ensure member matches predicate
            if (Predicate is not null && !Predicate(Member.Deserialize(this))) {
                continue;
            }

            // 
            Position = Member.StartValuePosition;

            // Ensure limit not reached
            if (CurrentDeletions >= MaxDeletions) {
                break;
            }
        }
        return CurrentDeletions;
    }
    public void ReadWhitespace() {
        while (true) {
            int Byte = PeekByte();

            // Whitespace
            if (Byte is ' ' or '\n' or '\r' or '\t' or '\v' or '\f') {
                ReadByte();
            }
            // End of whitespace
            else {
                return;
            }
        }
    }
    public IEnumerable<JsonStreamToken> ReadValue(IList<JsonStreamKey>? Path) {
        // Whitespace
        ReadWhitespace();

        int Byte = PeekByte();

        // End of stream
        if (Byte < 0) {
            throw new JsonException("Expected value");
        }
        // Object
        else if (Byte is '{') {
            return ReadObject(Path);
        }
        // Array
        else if (Byte is '[') {
            return ReadArray(Path);
        }
        // Primitive
        else {
            return [ReadPrimitiveValue()];
        }
    }
    public JsonStreamToken ReadPrimitiveValue() {
        // Whitespace
        ReadWhitespace();

        int Byte = PeekByte();

        // End of stream
        if (Byte < 0) {
            throw new JsonException("Expected value");
        }
        // Null
        else if (Byte is 'n') {
            return ReadNull();
        }
        // True
        else if (Byte is 't') {
            return ReadTrue();
        }
        // False
        else if (Byte is 'f') {
            return ReadFalse();
        }
        // String
        else if (Byte is '"') {
            return ReadString();
        }
        // Number
        else if (Byte is >= '0' and <= '9') {
            return ReadNumber();
        }
        // Unexpected byte
        else {
            throw new JsonException($"Invalid character in JSON: `{(char)Byte}`");
        }
    }
    public JsonStreamToken ReadNull() {
        // Null
        return ReadLiteralToken(JsonTokenType.Null, "null");
    }
    public JsonStreamToken ReadTrue() {
        // True
        return ReadLiteralToken(JsonTokenType.True, "true");
    }
    public JsonStreamToken ReadFalse() {
        // False
        return ReadLiteralToken(JsonTokenType.False, "false");
    }
    public JsonStreamToken ReadBoolean() {
        // Whitespace
        ReadWhitespace();

        int Byte = PeekByte();

        // True
        if (Byte is 't') {
            return ReadTrue();
        }
        // False
        else {
            return ReadFalse();
        }
    }
    public JsonStreamToken ReadString() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Opening quote
        if (!TryReadLiteralByte('"')) {
            throw new JsonException("Expected `\"` to start string");
        }

        // Start token
        StringBuilder?.Clear();

        while (true) {
            int Byte = ReadByte();

            // End of stream
            if (Byte < 0) {
                throw new JsonException("Expected `\"` to end string");
            }
            // Closing quote
            else if (Byte is '"') {
                return new JsonStreamToken(TokenPosition, JsonTokenType.String);
            }
            // Escape
            else if (Byte is '\\') {
                int EscapedByte = ReadByte();

                // End of stream
                if (EscapedByte < 0) {
                    throw new JsonException("Expected escape character after `\\`");
                }
                // Quote
                else if (EscapedByte is '"') {
                    StringBuilder?.Append('"');
                }
                // Backslash
                else if (EscapedByte is '\\') {
                    StringBuilder?.Append('\\');
                }
                // Slash
                else if (EscapedByte is '/') {
                    StringBuilder?.Append('/');
                }
                // Backspace
                else if (EscapedByte is 'b') {
                    StringBuilder?.Append('\b');
                }
                // Form feed
                else if (EscapedByte is 'f') {
                    StringBuilder?.Append('\f');
                }
                // New line
                else if (EscapedByte is 'n') {
                    StringBuilder?.Append('\n');
                }
                // Carriage return
                else if (EscapedByte is 'r') {
                    StringBuilder?.Append('\r');
                }
                // Tab
                else if (EscapedByte is 't') {
                    StringBuilder?.Append('\t');
                }
                // Unicode
                else if (EscapedByte is 'u') {
                    StringBuilder?.Append(ReadUnicodeCharacterFromHexadecimalSequence());
                }
                // Byte
                else {
                    throw new JsonException($"Expected valid escape character after `\\` (got `{(char)EscapedByte}`)");
                }
            }
            // Character
            else {
                ReadCharacter((byte)Byte);
            }
        }
    }
    public JsonStreamToken ReadInteger() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Start token
        StringBuilder?.Clear();

        // Sign
        bool HasSign = false;
        if (TryReadLiteralByte('-')) {
            StringBuilder?.Append('-');
            HasSign = true;
        }
        else if (TryReadLiteralByte('+')) {
            StringBuilder?.Append('+');
            HasSign = true;
        }

        // Integer
        bool TrailingSign = HasSign;
        while (true) {
            int Byte = PeekByte();

            // Digit
            if (Byte is >= '0' and <= '9') {
                TrailingSign = false;
                StringBuilder?.Append((char)Byte);
                ReadByte();
            }
            // End of number
            else {
                if (TrailingSign) {
                    throw new JsonException($"Expected digit after `+`/`-`");
                }
                return new JsonStreamToken(TokenPosition, JsonTokenType.Number);
            }
        }
    }
    public JsonStreamToken ReadNumber() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Integer
        ReadInteger();

        // Decimal point
        if (!TryReadLiteralByte('.')) {
            return new JsonStreamToken(TokenPosition, JsonTokenType.Number);
        }

        // Fraction
        bool IsExponent = false;
        bool TrailingDecimalPoint = true;
        bool TrailingExponent = false;
        while (true) {
            int Byte = PeekByte();

            // Exponent
            if (Byte is 'e' or 'E') {
                if (IsExponent) {
                    throw new JsonException($"Duplicate exponent: `{(char)Byte}`");
                }
                IsExponent = true;

                if (TrailingDecimalPoint) {
                    throw new JsonException($"Expected digit before `{(char)Byte}`");
                }

                TrailingExponent = true;
                StringBuilder?.Append((char)Byte);
                ReadByte();

                // Exponent sign
                if (TryReadLiteralByte('-')) {
                    StringBuilder?.Append('-');
                }
                else if (TryReadLiteralByte('+')) {
                    StringBuilder?.Append('+');
                }
            }
            // Digit
            else if (Byte is >= '0' and <= '9') {
                TrailingDecimalPoint = false;
                TrailingExponent = false;

                StringBuilder?.Append((char)Byte);
                ReadByte();
            }
            // End of number
            else {
                if (TrailingDecimalPoint) {
                    throw new JsonException("Expected digit after `.`");
                }
                if (TrailingExponent) {
                    throw new JsonException("Expected digit after `e`/`E`");
                }
                return new JsonStreamToken(TokenPosition, JsonTokenType.Number);
            }
        }
    }
    public JsonStreamToken ReadPropertyName() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // String
        ReadString();

        // Whitespace
        ReadWhitespace();

        // Colon
        if (!TryReadLiteralByte(':')) {
            throw new JsonException("Expected `:` after property name in object");
        }
        return new JsonStreamToken(TokenPosition, JsonTokenType.PropertyName);
    }
    public IEnumerable<JsonStreamToken> ReadObject(IList<JsonStreamKey>? Path) {
        // Whitespace
        ReadWhitespace();

        // Opening bracket
        if (!TryPeekLiteralByte('{')) {
            throw new JsonException("Expected `{` to start object");
        }
        yield return new JsonStreamToken(Position, JsonTokenType.StartObject);
        ReadByte();
        // Whitespace
        ReadWhitespace();

        bool AllowProperty = true;

        while (true) {
            int Byte = PeekByte();

            // Closing bracket
            if (Byte is '}') {
                yield return new JsonStreamToken(Position, JsonTokenType.EndObject);
                ReadByte();
                yield break;
            }
            // Property name
            else if (Byte is '"') {
                // Unexpected property name
                if (!AllowProperty) {
                    throw new JsonException("Expected `,` before property name in object");
                }

                // Property name
                yield return ReadPropertyName();
                // Whitespace
                ReadWhitespace();

                // Add property name to path
                if (StringBuilder is not null) {
                    Path?.Add(new JsonStreamKey(StringBuilder.ToString()));
                }

                // Property value
                foreach (JsonStreamToken Token in ReadValue(Path)) {
                    yield return Token;
                }
                // Whitespace
                ReadWhitespace();

                // Remove property name from path
                if (StringBuilder is not null) {
                    Path?.RemoveLast();
                }

                // Comma
                AllowProperty = TryReadLiteralByte(',');
                // Whitespace
                ReadWhitespace();
            }
            // Unexpected character
            else {
                throw new JsonException("Expected `}` to end object");
            }
        }
    }
    public IEnumerable<JsonStreamToken> ReadArray(IList<JsonStreamKey>? Path) {
        // Whitespace
        ReadWhitespace();

        // Opening bracket
        if (!TryPeekLiteralByte('[')) {
            throw new JsonException("Expected `[` to start array");
        }
        yield return new JsonStreamToken(Position, JsonTokenType.StartArray);
        ReadByte();
        // Whitespace
        ReadWhitespace();

        bool AllowItem = true;
        long CurrentIndex = 0;

        while (true) {
            int Byte = PeekByte();

            // End of stream
            if (Byte < 0) {
                throw new JsonException("Expected `]` to end array");
            }
            // Closing bracket
            else if (Byte is ']') {
                yield return new JsonStreamToken(Position, JsonTokenType.EndArray);
                ReadByte();
                yield break;
            }
            // Item
            else {
                // Unexpected item
                if (!AllowItem) {
                    throw new JsonException("Expected `,` before item in array");
                }

                // Add item to path
                if (StringBuilder is not null) {
                    Path?.Add(new JsonStreamKey(CurrentIndex));
                }

                // Item
                foreach (JsonStreamToken Token in ReadValue(Path)) {
                    yield return Token;
                }
                // Whitespace
                ReadWhitespace();

                // Remove item from path
                if (StringBuilder is not null) {
                    Path?.RemoveLast();
                }

                // Comma
                AllowItem = TryReadLiteralByte(',');
                // Whitespace
                ReadWhitespace();

                // Next array index
                CurrentIndex++;
            }
        }
    }

    private JsonStreamToken ReadLiteralToken(JsonTokenType TokenType, ReadOnlySpan<char> Literal) {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Literal
        foreach (char Char in Literal) {
            if (!TryReadLiteralByte(Char)) {
                throw new JsonException($"Expected `{Char}` in `{TokenType}`");
            }
        }
        return new JsonStreamToken(TokenPosition, TokenType);
    }
    private bool TryPeekLiteralByte(char Literal) {
        int Byte = PeekByte();
        return Byte == Literal;
    }
    private bool TryReadLiteralByte(char Literal) {
        if (TryPeekLiteralByte(Literal)) {
            ReadByte();
            return true;
        }
        return false;
    }
    private void ReadCharacter(byte FirstByte) {
        // ASCII character
        if (FirstByte <= 127) {
            StringBuilder?.Append((char)FirstByte);
        }
        // Multi-byte UTF8 character
        else {
            ReadUtf8Sequence(FirstByte);
        }
    }
    
    private void ReadCharacter() {
        int FirstByte = ReadByte();

        // End of stream
        if (FirstByte < 0) {
            throw new JsonException("Expected UTF8 character in string");
        }
        // Character
        else {
            ReadCharacter((byte)FirstByte);
        }
    }
    private char ReadUnicodeCharacterFromHexadecimalSequence() {
        Span<byte> HexBytes = stackalloc byte[4];

        for (int Index = 0; Index < HexBytes.Length; Index++) {
            int Byte = ReadByte();

            // End of stream
            if (Byte < 0) {
                throw new JsonException("Incomplete unicode character escape sequence");
            }
            // Hexadecimal byte
            else if (Byte is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                HexBytes[Index] = (byte)Byte;
            }
            // Unexpected byte
            else {
                throw new JsonException("Expected 4 hexadecimal digits for unicode character escape sequence");
            }
        }

        // Parse unicode character from 4 hexadecimal digits
        char UnicodeCharacter = (char)ushort.Parse(HexBytes, NumberStyles.AllowHexSpecifier);
        return UnicodeCharacter;
    }
    private void ReadUtf8Sequence(byte FirstByte) {
        // Get number of bytes in UTF8 character
        int SequenceLength = GetUtf8SequenceLength(FirstByte);

        // Store first byte
        Span<byte> Bytes = stackalloc byte[SequenceLength];
        Bytes[0] = FirstByte;

        // Read up to 4 bytes
        for (int Index = 1; Index < SequenceLength; Index++) {
            int Byte = ReadByte();

            // End of stream
            if (Byte < 0) {
                throw new JsonException("Expected byte in UTF8 character sequence in string");
            }
            // Add byte
            else {
                Bytes[Index] = (byte)Byte;
            }
        }
        // Parse up to 2 characters (to support surrogate pairs)
        Span<char> Chars = stackalloc char[2];
        if (!Encoding.UTF8.TryGetChars(Bytes, Chars, out int CharCount)) {
            throw new JsonException("Malformed bytes in UTF8 character sequence");
        }
        // Append character bytes
        StringBuilder?.Append(Chars[..CharCount]);
    }
    /// <summary>
    /// Gets the length of a single UTF8 character from its first byte.
    /// </summary>
    private static int GetUtf8SequenceLength(byte FirstByte) {
        // https://codegolf.stackexchange.com/a/173577
        return (FirstByte - 160 >> 20 - FirstByte / 16) + 2;
    }
}

public readonly record struct JsonStreamToken(long Position, JsonTokenType Type);
public readonly record struct JsonStreamMember(JsonStreamKey Key, JsonElement Value) {
    public byte[] SerializeToUtf8Bytes(bool AppendComma) {
        if (Key.PropertyName is not null) {
            return [
                .. Encoding.UTF8.GetBytes("\""),
                .. JsonSerializer.SerializeToUtf8Bytes(Key.PropertyName),
                .. Encoding.UTF8.GetBytes("\":"),
                .. JsonSerializer.SerializeToUtf8Bytes(Value),
                .. Encoding.UTF8.GetBytes(AppendComma ? "," : "")
            ];
        }
        else if (Key.ArrayIndex is not null) {
            return [
                .. JsonSerializer.SerializeToUtf8Bytes(Key.ArrayIndex.Value),
                .. Encoding.UTF8.GetBytes(AppendComma ? "," : "")
            ];
        }
        else {
            throw new InvalidOperationException("Key is empty");
        }
    }
}
public readonly record struct JsonStreamMemberRef(JsonStreamKey Key, long StartValuePosition, long EndValuePosition) {
    public T DeserializeValue<T>(HjsonStream JsonStream) {
        return JsonStream.DeserializeValueAt<T>(StartValuePosition, EndValuePosition);
    }
    public JsonElement DeserializeValue(HjsonStream JsonStream) {
        return DeserializeValue<JsonElement>(JsonStream);
    }
    public JsonStreamMember Deserialize(HjsonStream JsonStream) {
        return new JsonStreamMember(Key, DeserializeValue(JsonStream));
    }
}
public readonly record struct JsonStreamKey {
    public readonly string? PropertyName;
    public readonly long? ArrayIndex;

    public JsonStreamKey(string PropertyName) {
        this.PropertyName = PropertyName;
    }
    public JsonStreamKey(long ArrayIndex) {
        this.ArrayIndex = ArrayIndex;
    }

    public static implicit operator JsonStreamKey(string PropertyName)
        => new(PropertyName);
    public static implicit operator JsonStreamKey(long ArrayIndex)
        => new(ArrayIndex);
}