using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HjsonSharp;

/*
/// <summary>
/// A buffered layer for a <see cref="Stream"/> that can read and write UTF8-encoded JSON matching the <see href="https://json.org"/> specification.
/// </summary>
/// Supports parsing the following extra syntax:
/// <list type="bullet">
///   <item>Trailing commas (<c>,</c>)</item>
///   <item>Line breaks in strings (<c>\n</c>, <c>\r</c>)</item>
///   <item>Numbers starting with zero (<c>0</c>)</item>
///   <item>Numbers starting with a positive sign (<c>+</c>)</item>
/// </list>
*/
public class HjsonStream(Stream Stream, HjsonStreamOptions Options) : ByteStream(Stream, Options.BufferSize) {
    public HjsonStreamOptions Options { get; set; } = Options;

    private readonly StringBuilder StringBuilder = new();

    public HjsonStream(Stream Stream) : this(Stream, new HjsonStreamOptions()) {
    }
    public HjsonStream(string String, HjsonStreamOptions Options) : this(new MemoryStream(Encoding.UTF8.GetBytes(String)), Options) {
    }
    public HjsonStream(string String) : this(String, new HjsonStreamOptions()) {
    }
    public T? ParseElement<T>() {
        return ParseNode().Deserialize<T>();
    }
    public IEnumerable<Token> ReadElement() {
        // Whitespace
        ReadWhitespace();

        int Byte = PeekByte();

        // End of stream
        if (Byte < 0) {
            throw new HjsonException("Expected value");
        }
        // Object
        else if (Byte is '{') {
            return ReadObject();
        }
        // Array
        else if (Byte is '[') {
            return ReadArray();
        }
        // Primitive
        else {
            return [ReadPrimitiveElement()];
        }
    }
    public bool FindPath(IEnumerable<string> Path) {
        // Already at path if empty
        if (!Path.Any()) {
            return true;
        }

        Stack<string> CurrentPath = [];
        string? CurrentPropertyName = null;

        foreach (Token Token in ReadElement()) {
            // Start structure
            if (Token.Type is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                // Property value
                if (CurrentPropertyName is not null) {
                    // Enter path
                    CurrentPath.Push(CurrentPropertyName);

                    // Path found
                    if (CurrentPath.SequenceEqual(Path)) {
                        return true;
                    }

                    // Reset property name
                    CurrentPropertyName = null;
                }
            }
            // End structure
            else if (Token.Type is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                CurrentPath.Pop();
            }
            // Primitive value
            else if (Token.Type is JsonTokenType.String or JsonTokenType.Number or JsonTokenType.True or JsonTokenType.False or JsonTokenType.Null) {
                // Property value
                if (CurrentPropertyName is not null) {
                    // Path found
                    if (CurrentPath.Append(CurrentPropertyName).SequenceEqual(Path)) {
                        return true;
                    }

                    // Reset property name
                    CurrentPropertyName = null;
                }
            }
            // Property name
            else if (Token.Type is JsonTokenType.PropertyName) {
                CurrentPropertyName = StringBuilder.ToString();
            }
            // Comment
            else if (Token.Type is JsonTokenType.Comment) {
                // Pass
            }
            // Not implemented
            else {
                throw new NotImplementedException(Token.Type.ToString());
            }
        }

        // Path not found
        return false;
    }

    private JsonNode? ParseNode() {
        JsonNode? CurrentNode = null;
        string? CurrentPropertyName = null;

        bool SubmitNode(JsonNode? Node) {
            // Root value
            if (CurrentNode is null) {
                return true;
            }
            // Array item
            if (CurrentPropertyName is null) {
                CurrentNode.AsArray().Add(Node);
                return false;
            }
            // Object property
            else {
                CurrentNode.AsObject().Add(CurrentPropertyName, Node);
                CurrentPropertyName = null;
                return false;
            }
        }
        void SetNewCurrentNode(JsonNode NewCurrentNode) {
            SubmitNode(NewCurrentNode);
            CurrentNode = NewCurrentNode;
        }

        foreach (Token Token in ReadElement()) {
            // Null
            if (Token.Type is JsonTokenType.Null) {
                JsonValue? Node = null;
                if (SubmitNode(Node)) {
                    return null;
                }
            }
            // True
            if (Token.Type is JsonTokenType.True) {
                JsonValue Node = JsonValue.Create(true);
                if (SubmitNode(Node)) {
                    return null;
                }
            }
            // False
            if (Token.Type is JsonTokenType.False) {
                JsonValue Node = JsonValue.Create(false);
                if (SubmitNode(Node)) {
                    return null;
                }
            }
            // String
            else if (Token.Type is JsonTokenType.String) {
                JsonValue Node = JsonValue.Create(Token.Value);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // Number
            else if (Token.Type is JsonTokenType.Number) {
                JsonValue Node = JsonValue.Create(Token.Value);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // Start Object
            else if (Token.Type is JsonTokenType.StartObject) {
                JsonObject Node = [];
                SetNewCurrentNode(Node);
            }
            // Start Array
            else if (Token.Type is JsonTokenType.StartArray) {
                JsonArray Node = [];
                SetNewCurrentNode(Node);
            }
            // End Object/Array
            else if (Token.Type is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                // Nested node
                if (CurrentNode?.Parent is not null) {
                    CurrentNode = CurrentNode.Parent;
                }
                // Root node
                else {
                    return CurrentNode;
                }
            }
            // Property Name
            else if (Token.Type is JsonTokenType.PropertyName) {
                CurrentPropertyName = Token.Value;
            }
            // Comment
            else if (Token.Type is JsonTokenType.Comment) {
                // Pass
            }
            // Not implemented
            else {
                throw new NotImplementedException(Token.Type.ToString());
            }
        }

        // End of stream
        throw new HjsonException("Expected token");
    }
    private void ReadWhitespace() {
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
    private Token ReadPrimitiveElement() {
        // Whitespace
        ReadWhitespace();

        int Byte = PeekByte();

        // End of stream
        if (Byte < 0) {
            throw new HjsonException("Expected value");
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
            throw new HjsonException($"Invalid character in JSON: `{(char)Byte}`");
        }
    }
    private Token ReadNull() {
        // Null
        return ReadLiteralToken(JsonTokenType.Null, "null");
    }
    private Token ReadTrue() {
        // True
        return ReadLiteralToken(JsonTokenType.True, "true");
    }
    private Token ReadFalse() {
        // False
        return ReadLiteralToken(JsonTokenType.False, "false");
    }
    private Token ReadBoolean() {
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
    private Token ReadString() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Opening quote
        if (!TryReadLiteralByte('"')) {
            throw new HjsonException("Expected `\"` to start string");
        }

        // Start token
        StringBuilder.Clear();

        while (true) {
            int Byte = ReadByte();

            // End of stream
            if (Byte < 0) {
                throw new HjsonException("Expected `\"` to end string");
            }
            // Closing quote
            else if (Byte is '"') {
                return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
            }
            // Escape
            else if (Byte is '\\') {
                int EscapedByte = ReadByte();

                // End of stream
                if (EscapedByte < 0) {
                    throw new HjsonException("Expected escape character after `\\`");
                }
                // Quote
                else if (EscapedByte is '"') {
                    StringBuilder.Append('"');
                }
                // Backslash
                else if (EscapedByte is '\\') {
                    StringBuilder.Append('\\');
                }
                // Slash
                else if (EscapedByte is '/') {
                    StringBuilder.Append('/');
                }
                // Backspace
                else if (EscapedByte is 'b') {
                    StringBuilder.Append('\b');
                }
                // Form feed
                else if (EscapedByte is 'f') {
                    StringBuilder.Append('\f');
                }
                // New line
                else if (EscapedByte is 'n') {
                    StringBuilder.Append('\n');
                }
                // Carriage return
                else if (EscapedByte is 'r') {
                    StringBuilder.Append('\r');
                }
                // Tab
                else if (EscapedByte is 't') {
                    StringBuilder.Append('\t');
                }
                // Unicode
                else if (EscapedByte is 'u') {
                    StringBuilder.Append(ReadCharFromHexadecimalSequence());
                }
                // Invalid escape character
                else {
                    throw new HjsonException($"Expected valid escape character after `\\` (got `{(char)EscapedByte}`)");
                }
            }
            // Character
            else {
                ReadUtf8Character((byte)Byte);
            }
        }
    }
    private Token ReadNumber() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Integer
        Token Integer = ReadInteger();

        // Decimal point
        if (!TryReadLiteralByte('.')) {
            return new Token(this, JsonTokenType.Number, TokenPosition, Position - TokenPosition, Integer.Value);
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
                    throw new HjsonException($"Duplicate exponent: `{(char)Byte}`");
                }
                IsExponent = true;

                if (TrailingDecimalPoint) {
                    throw new HjsonException($"Expected digit before `{(char)Byte}`");
                }

                TrailingExponent = true;
                StringBuilder.Append((char)Byte);
                ReadByte();

                // Exponent sign
                if (TryReadLiteralByte('-')) {
                    StringBuilder.Append('-');
                }
                else if (TryReadLiteralByte('+')) {
                    StringBuilder.Append('+');
                }
            }
            // Digit
            else if (Byte is >= '0' and <= '9') {
                TrailingDecimalPoint = false;
                TrailingExponent = false;

                StringBuilder.Append((char)Byte);
                ReadByte();
            }
            // End of number
            else {
                if (TrailingDecimalPoint) {
                    throw new HjsonException("Expected digit after `.`");
                }
                if (TrailingExponent) {
                    throw new HjsonException("Expected digit after `e`/`E`");
                }
                return new Token(this, JsonTokenType.Number, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
            }
        }
    }
    private Token ReadInteger() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Start token
        StringBuilder.Clear();

        // Sign
        bool HasSign = false;
        if (TryReadLiteralByte('-')) {
            StringBuilder.Append('-');
            HasSign = true;
        }
        else if (TryReadLiteralByte('+')) {
            StringBuilder.Append('+');
            HasSign = true;
        }

        // Integer
        bool TrailingSign = HasSign;
        while (true) {
            int Byte = PeekByte();

            // Digit
            if (Byte is >= '0' and <= '9') {
                TrailingSign = false;
                StringBuilder.Append((char)Byte);
                ReadByte();
            }
            // End of number
            else {
                if (TrailingSign) {
                    throw new HjsonException($"Expected digit after `+`/`-`");
                }
                return new Token(this, JsonTokenType.Number, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
            }
        }
    }
    private IEnumerable<Token> ReadObject() {
        // Whitespace
        ReadWhitespace();

        // Opening bracket
        if (!TryPeekLiteralByte('{')) {
            throw new HjsonException("Expected `{` to start object");
        }
        yield return new Token(this, JsonTokenType.StartObject, Position, 1, "");
        ReadByte();
        // Whitespace
        ReadWhitespace();

        bool AllowProperty = true;

        while (true) {
            int Byte = PeekByte();

            // Closing bracket
            if (Byte is '}') {
                yield return new Token(this, JsonTokenType.EndObject, Position, 1, "");
                ReadByte();
                yield break;
            }
            // Property name
            else if (Byte is '"') {
                // Unexpected property name
                if (!AllowProperty) {
                    throw new HjsonException("Expected `,` before property name in object");
                }

                // Property name
                yield return ReadPropertyName();
                // Whitespace
                ReadWhitespace();

                // Property value
                foreach (Token Token in ReadElement()) {
                    yield return Token;
                }
                // Whitespace
                ReadWhitespace();

                // Comma
                AllowProperty = TryReadLiteralByte(',');
                // Whitespace
                ReadWhitespace();
            }
            // Unexpected character
            else {
                throw new HjsonException("Expected `}` to end object");
            }
        }
    }
    private Token ReadPropertyName() {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // String
        Token String = ReadString();

        // Whitespace
        ReadWhitespace();

        // Colon
        if (!TryReadLiteralByte(':')) {
            throw new HjsonException("Expected `:` after property name in object");
        }
        return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, String.Value);
    }
    private IEnumerable<Token> ReadArray() {
        // Whitespace
        ReadWhitespace();

        // Opening bracket
        if (!TryPeekLiteralByte('[')) {
            throw new HjsonException("Expected `[` to start array");
        }
        yield return new Token(this, JsonTokenType.StartArray, Position, 1, "");
        ReadByte();
        // Whitespace
        ReadWhitespace();

        bool AllowItem = true;
        long CurrentIndex = 0;

        while (true) {
            int Byte = PeekByte();

            // End of stream
            if (Byte < 0) {
                throw new HjsonException("Expected `]` to end array");
            }
            // Closing bracket
            else if (Byte is ']') {
                yield return new Token(this, JsonTokenType.EndArray, Position, 1, "");
                ReadByte();
                yield break;
            }
            // Item
            else {
                // Unexpected item
                if (!AllowItem) {
                    throw new HjsonException("Expected `,` before item in array");
                }

                // Item
                foreach (Token Token in ReadElement()) {
                    yield return Token;
                }
                // Whitespace
                ReadWhitespace();

                // Comma
                AllowItem = TryReadLiteralByte(',');
                // Whitespace
                ReadWhitespace();

                // Next array index
                CurrentIndex++;
            }
        }
    }
    private Token ReadLiteralToken(JsonTokenType TokenType, string Literal) {
        // Whitespace
        ReadWhitespace();
        long TokenPosition = Position;

        // Literal
        foreach (char Char in Literal) {
            if (!TryReadLiteralByte(Char)) {
                throw new HjsonException($"Expected `{Char}` in `{TokenType}`");
            }
        }
        return new Token(this, TokenType, TokenPosition, Literal.Length, "");
    }
    private bool TryPeekLiteralByte(char Literal) {
        int Byte = PeekByte();
        return Byte == Literal;
    }
    private bool TryReadLiteralByte(char LiteralByte) {
        if (TryPeekLiteralByte(LiteralByte)) {
            ReadByte();
            return true;
        }
        return false;
    }
    private char ReadCharFromHexadecimalSequence() {
        Span<byte> HexBytes = stackalloc byte[4];

        for (int Index = 0; Index < HexBytes.Length; Index++) {
            int Byte = ReadByte();

            // End of stream
            if (Byte < 0) {
                throw new HjsonException("Incomplete unicode escape sequence");
            }
            // Hexadecimal byte
            else if (Byte is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                HexBytes[Index] = (byte)Byte;
            }
            // Unexpected byte
            else {
                throw new HjsonException("Expected 4 hexadecimal digits for unicode escape sequence");
            }
        }

        // Parse unicode character from 4 hexadecimal digits
        char UnicodeCharacter = (char)ushort.Parse(HexBytes, NumberStyles.AllowHexSpecifier);
        return UnicodeCharacter;
    }
    private void ReadUtf8Character(byte FirstByte) {
        // Single-byte ASCII character
        if (FirstByte <= 127) {
            StringBuilder.Append((char)FirstByte);
            return;
        }

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
                throw new HjsonException("Expected byte in UTF8 character sequence in string");
            }
            // Add byte
            else {
                Bytes[Index] = (byte)Byte;
            }
        }
        // Parse up to 2 characters (to support surrogate pairs)
        Span<char> Chars = stackalloc char[2];
        if (!Encoding.UTF8.TryGetChars(Bytes, Chars, out int CharCount)) {
            throw new HjsonException("Malformed bytes in UTF8 character sequence");
        }
        // Append character bytes
        StringBuilder.Append(Chars[..CharCount]);
    }
    /// <summary>
    /// Gets the length of a single UTF8 character from its first byte.
    /// </summary>
    private static int GetUtf8SequenceLength(byte FirstByte) {
        // https://codegolf.stackexchange.com/a/173577
        return (FirstByte - 160 >> 20 - FirstByte / 16) + 2;
    }

    public readonly record struct Token(HjsonStream HjsonStream, JsonTokenType Type, long Position, long Length, string Value) {
        public T? ToElement<T>() {
            // Go to token position
            long OriginalPosition = HjsonStream.Position;
            HjsonStream.Position = Position;

            try {
                // Parse element
                return HjsonStream.ParseElement<T>();
            }
            finally {
                // Return to original position
                HjsonStream.Position = OriginalPosition;
            }
        }
    }
}