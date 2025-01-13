using LinkDotNet.StringBuilder;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace HjsonSharp;

public sealed class HjsonStream : RuneStream {
    public HjsonStreamOptions Options { get; set; }

    public HjsonStream(Stream Stream, HjsonStreamOptions? Options = null)
        : base(new BufferedStream(Stream)) {
        this.Options = Options ?? HjsonStreamOptions.Hjson;
    }
    public HjsonStream(byte[] Bytes, HjsonStreamOptions? Options = null)
        : this(new MemoryStream(Bytes), Options) {
    }
    public HjsonStream(string String, HjsonStreamOptions? Options = null)
        : this((Options?.StreamEncoding ?? Encoding.UTF8).GetBytes(String), Options) {
    }

    public static T? ParseElement<T>(Stream Stream, HjsonStreamOptions? Options = null) {
        using HjsonStream HjsonStream = new(Stream, Options);
        return HjsonStream.ParseElement<T>();
    }
    public static JsonElement ParseElement(Stream Stream, HjsonStreamOptions? Options = null) {
        return ParseElement<JsonElement>(Stream, Options);
    }
    public static T? ParseElement<T>(byte[] Bytes, HjsonStreamOptions? Options = null) {
        using HjsonStream HjsonStream = new(Bytes, Options);
        return HjsonStream.ParseElement<T>();
    }
    public static JsonElement ParseElement(byte[] Bytes, HjsonStreamOptions? Options = null) {
        return ParseElement<JsonElement>(Bytes, Options);
    }
    public static T? ParseElement<T>(string String, HjsonStreamOptions? Options = null) {
        using HjsonStream HjsonStream = new(String, Options);
        return HjsonStream.ParseElement<T>();
    }
    public static JsonElement ParseElement(string String, HjsonStreamOptions? Options = null) {
        return ParseElement<JsonElement>(String, Options);
    }

    public T? ParseElement<T>() {
        return ParseNode().Deserialize<T>(JsonOptions.Mini);
    }
    public JsonElement ParseElement() {
        return ParseElement<JsonElement>();
    }
    public bool TryParseElement<T>(out T? Result) {
        try {
            Result = ParseElement<T>();
            return true;
        }
        catch (Exception) {
            Result = default;
            return false;
        }
    }
    public bool TryParseElement(out JsonElement Result) {
        return TryParseElement<JsonElement>(out Result);
    }
    public JsonNode? ParseNode() {
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
                    return Node;
                }
            }
            // True
            if (Token.Type is JsonTokenType.True) {
                JsonValue Node = JsonValue.Create(true);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // False
            if (Token.Type is JsonTokenType.False) {
                JsonValue Node = JsonValue.Create(false);
                if (SubmitNode(Node)) {
                    return Node;
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
        throw new HjsonException("Expected token, got end of stream");
    }
    public bool TryParseNode(out JsonNode? Result) {
        try {
            Result = ParseNode();
            return true;
        }
        catch (Exception) {
            Result = null;
            return false;
        }
    }
    public IEnumerable<Token> ReadElement() {
        // Comments & whitespace
        foreach (Token Token in ReadCommentsAndWhitespace()) {
            yield return Token;
        }

        // Peek rune
        if (PeekRune() is not Rune Rune) {
            throw new HjsonException("Expected token, got end of stream");
        }

        // Object
        if (Rune.Value is '{') {
            foreach (Token Token in ReadObject()) {
                yield return Token;
            }
        }
        // Array
        else if (Rune.Value is '[') {
            foreach (Token Token in ReadArray()) {
                yield return Token;
            }
        }
        // Primitive
        else {
            yield return ReadPrimitiveElement();
        }
    }
    public bool FindPath(string PropertyName) {
        long CurrentDepth = 0;

        foreach (Token Token in ReadElement()) {
            // Start structure
            if (Token.Type is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                CurrentDepth++;
            }
            // End structure
            else if (Token.Type is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                CurrentDepth--;
            }
            // Property name
            else if (Token.Type is JsonTokenType.PropertyName) {
                if (CurrentDepth == 1 && Token.Value == PropertyName) {
                    // Path found
                    return true;
                }
            }
        }

        // Path not found
        return false;
    }
    public bool FindPath(long ArrayIndex) {
        long CurrentDepth = 0;
        long CurrentIndex = -1;
        bool IsArray = false;

        foreach (Token Token in ReadElement()) {
            // Start structure
            if (Token.Type is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                CurrentDepth++;
                if (CurrentDepth == 1) {
                    IsArray = Token.Type is JsonTokenType.StartArray;
                }
            }
            // End structure
            else if (Token.Type is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                CurrentDepth--;
            }
            // Primitive value
            else if (Token.Type is JsonTokenType.Null or JsonTokenType.True or JsonTokenType.False or JsonTokenType.String or JsonTokenType.Number) {
                if (CurrentDepth == 1 && IsArray) {
                    CurrentIndex++;
                    // Path found
                    if (CurrentIndex == ArrayIndex) {
                        Position = Token.Position;
                        return true;
                    }
                }
            }
        }

        // Path not found
        return false;
    }
    public Rune? ReadRune() {
        if (Options.StreamEncoding is null) {
            Options = Options with { StreamEncoding = DetectEncoding() };
        }
        return ReadRune(Options.StreamEncoding);
    }
    public Rune? PeekRune() {
        if (Options.StreamEncoding is null) {
            Options = Options with { StreamEncoding = DetectEncoding() };
        }
        return PeekRune(Options.StreamEncoding);
    }
    public bool ReadRune(Rune? Expected) {
        if (PeekRune() != Expected) {
            return false;
        }
        ReadRune();
        return true;
    }
    public bool ReadRune(char Expected) {
        return ReadRune(new Rune(Expected));
    }

    protected override void Dispose(bool Disposing) {
        if (Disposing) {
            if (!Options.LeaveStreamOpen) {
                InnerStream.Dispose();
            }
        }
    }

    private Token ReadPrimitiveElement() {
        // Peek rune
        if (PeekRune() is not Rune Rune) {
            throw new HjsonException("Expected token, got end of stream");
        }

        // Null
        if (Rune.Value is 'n') {
            return ReadNull();
        }
        // Boolean
        else if (Rune.Value is 't' or 'f') {
            return ReadBoolean();
        }
        // String
        else if (Rune.Value is '"' or '\'') {
            return ReadString();
        }
        // Number
        else if (Rune.Value is (>= '0' and <= '9') or ('-' or '+') or '.' or ('I' or 'N')) {
            return ReadNumber();
        }
        // Unquoted string
        else if (Options.UnquotedStrings) {
            return ReadUnquotedString();
        }
        // Invalid rune
        else {
            throw new HjsonException($"Invalid rune: `{Rune}`");
        }
    }
    private Token ReadNull() {
        // Null
        return ReadLiteralToken(JsonTokenType.Null, "null", out _);
    }
    private Token ReadBoolean() {
        // Peek rune
        if (PeekRune() is not Rune Rune) {
            throw new HjsonException("Expected boolean, got end of stream");
        }

        // True
        if (Rune.Value is 't') {
            return ReadLiteralToken(JsonTokenType.True, "true", out _);
        }
        // False
        else {
            return ReadLiteralToken(JsonTokenType.False, "false", out _);
        }
    }
    private Token ReadString() {
        long TokenPosition = Position;

        // Opening quote
        char OpeningQuote;
        // Double quote
        if (ReadRune('"')) {
            OpeningQuote = '"';
        }
        else {
            // Single quote
            if (ReadRune('\'')) {
                if (!Options.SingleQuotedStrings) {
                    throw new HjsonException("Single-quoted strings are not allowed");
                }
                OpeningQuote = '\'';
            }
            // Unquoted
            else {
                if (!Options.UnquotedStrings) {
                    throw new HjsonException("Unquoted strings are not allowed");
                }
                return ReadUnquotedString();
            }
        }

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Read rune
            if (ReadRune() is not Rune Rune) {
                throw new HjsonException($"Expected `{OpeningQuote}` to end string, got end of stream");
            }

            // Closing quote
            if (Rune.Value == OpeningQuote) {
                break;
            }
            // Escape
            else if (Rune.Value is '\\') {
                // Read escaped rune
                if (ReadRune() is not Rune EscapedRune) {
                    throw new HjsonException("Expected escape character after `\\`, got end of stream");
                }

                // Double quote
                if (EscapedRune.Value is '"') {
                    StringBuilder.Append('"');
                }
                // Single quote
                else if (EscapedRune.Value is '\'') {
                    StringBuilder.Append('\'');
                }
                // Backslash
                else if (EscapedRune.Value is '\\') {
                    StringBuilder.Append('\\');
                }
                // Slash
                else if (EscapedRune.Value is '/') {
                    StringBuilder.Append('/');
                }
                // Backspace
                else if (EscapedRune.Value is 'b') {
                    StringBuilder.Append('\b');
                }
                // Form feed
                else if (EscapedRune.Value is 'f') {
                    StringBuilder.Append('\f');
                }
                // Line feed
                else if (EscapedRune.Value is 'n') {
                    StringBuilder.Append('\n');
                }
                // Carriage return
                else if (EscapedRune.Value is 'r') {
                    StringBuilder.Append('\r');
                }
                // Horizontal tab
                else if (EscapedRune.Value is 't') {
                    StringBuilder.Append('\t');
                }
                // Vertical tab
                else if (EscapedRune.Value is 'v') {
                    StringBuilder.Append('\v');
                }
                // Unicode hex sequence
                else if (EscapedRune.Value is 'u') {
                    StringBuilder.Append(ReadCharFromHexSequence());
                }
                // Invalid escape character
                else {
                    throw new HjsonException($"Expected valid escape character after `\\`, got `{EscapedRune}`");
                }
            }
            // Rune
            else {
                StringBuilder.Append(Rune);
            }
        }

        // End token
        return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private Token ReadUnquotedString() {
        long TokenPosition = Position;

        // Start token
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Read rune
            if (ReadRune() is not Rune Rune) {
                break;
            }

            // Newline
            if (Rune.Value is '\n' or '\r') {
                break;
            }
            // Rune
            else {
                StringBuilder.Append(Rune);
            }
        }

        // End token
        return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private Token ReadNumber() {
        long TokenPosition = Position;

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        bool ParsedExponent = false;
        bool ParsedDecimalPoint = false;
        bool ParsedDigit = false;
        bool TrailingExponent = false;
        bool TrailingDecimalPoint = false;
        bool TrailingSign = false;

        // Sign
        if (ReadRune('-')) {
            StringBuilder.Append('-');
            TrailingSign = true;
        }
        else if (ReadRune('+')) {
            if (!Options.ExplicitPlusSigns) {
                throw new HjsonException("Explicit plus-signs are not allowed");
            }
            StringBuilder.Append('+');
            TrailingSign = true;
        }

        // Named floating point literal
        if (Options.NamedFloatingPointLiterals) {
            if (PeekRune() is Rune LiteralRune && LiteralRune.Value is 'I' or 'N') {
                // Guess full literal
                string Literal = LiteralRune.Value is 'I' ? "Infinity" : "NaN";
                // Read full literal
                Token LiteralToken = ReadLiteralToken(JsonTokenType.String, Literal, out bool UnquotedStringFallback);

                // Unquoted string read
                if (UnquotedStringFallback) {
                    StringBuilder.Append(LiteralToken.Value);
                }
                // Full literal read
                else {
                    StringBuilder.Append(Literal);
                }
                
                // Submit string token
                return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
            }
        }

        // Leading decimal point
        if (ReadRune('.')) {
            if (Options.LeadingDecimalPoints) {
                TrailingSign = false;
                TrailingDecimalPoint = true;

                StringBuilder.Append('.');
            }
            else if (Options.UnquotedStrings) {
                Position = TokenPosition;
                return ReadUnquotedString();
            }
            else {
                throw new HjsonException("Leading decimal points are not allowed");
            }
        }

        while (true) {
            // Peek rune
            Rune? Rune = PeekRune();

            // Digit
            if (Rune?.Value is >= '0' and <= '9') {
                if (!ParsedDigit && Rune.Value.Value is '0') {
                    if (!Options.LeadingZeroes) {
                        throw new HjsonException("Leading zeroes are not allowed");
                    }
                }

                ParsedDigit = true;

                TrailingExponent = false;
                TrailingDecimalPoint = false;
                TrailingSign = false;

                ReadRune();
                StringBuilder.Append(Rune.Value);
            }
            // Exponent
            else if (Rune?.Value is 'e' or 'E') {
                if (ParsedExponent) {
                    throw new HjsonException($"Duplicate exponent: `{Rune}`");
                }
                if (TrailingSign) {
                    throw new HjsonException($"Expected digit before exponent: `{Rune}`");
                }

                ParsedExponent = true;

                TrailingExponent = true;
                TrailingDecimalPoint = false;

                ReadRune();
                StringBuilder.Append(Rune.Value);

                // Exponent sign
                if (ReadRune('-')) {
                    StringBuilder.Append('-');
                }
                else if (ReadRune('+')) {
                    StringBuilder.Append('+');
                }
            }
            // Decimal point
            else if (Rune?.Value is '.') {
                if (ParsedDecimalPoint) {
                    throw new HjsonException($"Duplicate decimal point: `{Rune}`");
                }
                if (ParsedExponent) {
                    throw new HjsonException($"Exponent cannot be fractional: `{Rune}`");
                }

                ParsedDecimalPoint = true;

                TrailingExponent = false;
                TrailingDecimalPoint = true;
                TrailingSign = false;

                ReadRune();
                StringBuilder.Append('.');
            }
            // End of number
            else {
                if (TrailingDecimalPoint) {
                    if (!Options.TrailingDecimalPoints) {
                        if (Options.UnquotedStrings) {
                            Position = TokenPosition;
                            return ReadUnquotedString();
                        }
                        throw new HjsonException("Expected digit after decimal point");
                    }
                }
                if (TrailingExponent) {
                    if (Options.UnquotedStrings) {
                        Position = TokenPosition;
                        return ReadUnquotedString();
                    }
                    throw new HjsonException("Expected digit after exponent");
                }
                if (TrailingSign) {
                    if (Options.UnquotedStrings) {
                        Position = TokenPosition;
                        return ReadUnquotedString();
                    }
                    throw new HjsonException("Expected digit after sign");
                }

                // Detect unquoted string (e.g. `123 a`)
                if (Options.UnquotedStrings) {
                    if (DetectFallbackToUnquotedString()) {
                        Position = TokenPosition;
                        return ReadUnquotedString();
                    }
                }

                // End of number
                break;
            }
        }

        // End of number
        return new Token(this, JsonTokenType.Number, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private IEnumerable<Token> ReadObject() {
        // Opening bracket
        if (!ReadRune('{')) {
            throw new HjsonException($"Expected `{{` to start object");
        }
        yield return new Token(this, JsonTokenType.StartObject, Position - 1);

        // Comments & whitespace
        foreach (Token Token in ReadCommentsAndWhitespace()) {
            yield return Token;
        }

        bool PropertyLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (PeekRune() is not Rune Rune) {
                throw new HjsonException("Expected '}' to end object, got end of stream");
            }

            // Closing bracket
            if (Rune.Value is '}') {
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        throw new HjsonException("Trailing commas are not allowed");
                    }
                }
                // End of object
                yield return new Token(this, JsonTokenType.EndObject, Position);
                ReadRune();
                yield break;
            }
            // Property name
            else {
                // Unexpected property name
                if (!PropertyLegal) {
                    throw new HjsonException("Expected `,` before property name in object");
                }

                // Property name
                foreach (Token Token in ReadPropertyName()) {
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Token Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                }

                // Property value
                foreach (Token Token in ReadElement()) {
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Token Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                }

                // Comma
                TrailingComma = ReadRune(',');
                PropertyLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (Token Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                }
            }
        }
    }
    private IEnumerable<Token> ReadPropertyName() {
        long TokenPosition = Position;

        // Unquoted property name
        if (PeekRune()?.Value is not ('"' or '\'')) {
            if (Options.EcmaScriptPropertyNames) {
                yield return ReadEcmaScriptPropertyName();
                yield break;
            }
            else if (Options.UnquotedPropertyNames) {
                yield return ReadUnquotedPropertyName();
                yield break;
            }
            else {
                throw new HjsonException("Unquoted property names are not allowed");
            }
        }

        // String
        Token String = ReadString();

        // Comments & whitespace
        foreach (Token Token in ReadCommentsAndWhitespace()) {
            yield return Token;
        }

        // Colon
        if (!ReadRune(':')) {
            throw new HjsonException($"Expected `:` after property name in object");
        }
        yield return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, String.Value);
    }
    private Token ReadEcmaScriptPropertyName() {
        long TokenPosition = Position;

        // Start token
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Peek rune
            if (PeekRune() is not Rune Rune) {
                break;
            }

            // Colon
            if (Rune.Value is ':') {
                ReadRune();
                break;
            }
            // Comments & whitespace
            else if (Rune.Value is '#' or '/' || Rune.IsWhiteSpace(Rune)) {
                ReadCommentsAndWhitespace();

                // Colon
                if (!ReadRune(':')) {
                    throw new HjsonException($"Expected `:` after property name in object");
                }
                break;
            }
            // Dollar sign
            else if (Rune.Value is '$') {
                ReadRune();
                StringBuilder.Append('$');
            }
            // Underscore
            else if (Rune.Value is '_') {
                ReadRune();
                StringBuilder.Append('_');
            }
            // Escape
            else if (Rune.Value is '\\') {
                ReadRune();
                // Read escaped rune
                if (ReadRune() is not Rune EscapedRune) {
                    throw new HjsonException("Expected escape character after `\\`, got end of stream");
                }

                // Unicode hex sequence
                if (EscapedRune.Value is 'u') {
                    StringBuilder.Append(ReadCharFromHexSequence());
                }
                // Invalid escape character
                else {
                    throw new HjsonException($"Expected valid escape character after `\\`, got `{EscapedRune}`");
                }
            }
            // Unicode letter
            else if (Rune.IsLetter(Rune)) {
                ReadRune();
                StringBuilder.Append(Rune);
            }
            // Invalid rune
            else {
                throw new HjsonException($"Unexpected rune in property name: `{Rune}`");
            }
        }

        // End token
        return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private Token ReadUnquotedPropertyName() {
        long TokenPosition = Position;

        // Start token
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Peek rune
            if (PeekRune() is not Rune Rune) {
                break;
            }

            // Colon
            if (Rune.Value is ':') {
                ReadRune();
                break;
            }
            // Invalid rune
            else if (Rune.Value is ',' or ':' or '[' or ']' or '{' or '}' || Rune.IsWhiteSpace(Rune)) {
                throw new HjsonException($"Unexpected rune in property name: `{Rune}`");
            }
            // Valid rune
            else {
                ReadRune();
                StringBuilder.Append(Rune);
            }
        }

        // End token
        return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private IEnumerable<Token> ReadArray() {
        // Opening bracket
        if (!ReadRune('[')) {
            throw new HjsonException($"Expected `[` to start array");
        }
        yield return new Token(this, JsonTokenType.StartArray, Position - 1);

        // Comments & whitespace
        foreach (Token Token in ReadCommentsAndWhitespace()) {
            yield return Token;
        }

        bool ItemLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (PeekRune() is not Rune Rune) {
                throw new HjsonException("Expected `]` to end array, got end of stream");
            }

            // Closing bracket
            if (Rune.Value is ']') {
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        throw new HjsonException("Trailing commas are not allowed");
                    }
                }
                // End of array
                yield return new Token(this, JsonTokenType.EndArray, Position);
                ReadRune();
                yield break;
            }
            // Item
            else {
                // Unexpected item
                if (!ItemLegal) {
                    throw new HjsonException("Expected `,` before item in array");
                }

                // Item
                foreach (Token Token in ReadElement()) {
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Token Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                }

                // Comma
                TrailingComma = ReadRune(',');
                ItemLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (Token Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                }
            }
        }
    }
    private IEnumerable<Token> ReadCommentsAndWhitespace() {
        while (true) {
            // Whitespace
            ReadWhitespace();

            // Peek rune
            if (PeekRune() is not Rune Rune) {
                yield break;
            }

            // Hash-style comment
            if (Rune.Value is '#' or '/') {
                yield return ReadComment();
            }
            // End of comments
            else {
                yield break;
            }
        }
    }
    private Token ReadComment() {
        long TokenPosition = Position;

        // Comment type
        bool IsBlockComment = false;
        if (ReadRune('/')) {
            // Line-style comment
            if (ReadRune('/')) {
                // Ensure line-style comments are enabled
                if (!Options.LineStyleComments) {
                    throw new HjsonException("Line-style comments are not allowed");
                }
            }
            // Block-style comment
            else if (ReadRune('*')) {
                // Ensure block-style comments are enabled
                if (!Options.BlockStyleComments) {
                    throw new HjsonException("Block-style comments are not allowed");
                }
                IsBlockComment = true;
            }
        }
        // Hash-style comment
        else if (ReadRune('#')) {
            // Ensure hash-style comments are enabled
            if (!Options.HashStyleComments) {
                throw new HjsonException("Hash-style comments are not allowed");
            }
        }
        // Invalid comment
        else {
            throw new HjsonException($"Expected `#` or `//` or `/*` to start comment");
        }

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        // Read comment
        while (true) {
            // Read rune
            if (ReadRune() is not Rune CommentRune) {
                if (IsBlockComment) {
                    throw new HjsonException("Expected `*/` to end block-style comment, got end of stream");
                }
                break;
            }

            // Check end of block comment
            if (IsBlockComment) {
                if (CommentRune.Value is '*' && PeekRune()?.Value is '/') {
                    ReadRune();
                    break;
                }
            }
            // Check end of line comment
            else {
                if (CommentRune.Value is '\n' or '\r') {
                    break;
                }
            }

            // Append rune to comment
            StringBuilder.Append(CommentRune.Value);
        }

        // Create comment token
        return new Token(this, JsonTokenType.Comment, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private void ReadWhitespace() {
        while (true) {
            // Peek rune
            if (PeekRune() is not Rune Rune) {
                return;
            }

            // JSON whitespace
            if (Rune.Value is ' ' or '\n' or '\r' or '\t') {
                ReadRune();
            }
            // All whitespace
            else if (Rune.IsWhiteSpace(Rune)) {
                if (!Options.AllWhitespace) {
                    throw new HjsonException("Non-JSON whitespace is not allowed");
                }
                ReadRune();
            }
            // End of whitespace
            else {
                return;
            }
        }
    }
    private Token ReadLiteralToken(JsonTokenType TokenType, ReadOnlySpan<char> Literal, out bool UnquotedStringFallback) {
        long TokenPosition = Position;

        // Literal
        foreach (Rune ExpectedRune in Literal.EnumerateRunes()) {
            // Read rune
            Rune? ActualRune = ReadRune();

            // Expected rune
            if (ActualRune == ExpectedRune) {
                continue;
            }
            // Unquoted string
            else {
                if (!Options.UnquotedStrings) {
                    throw new HjsonException("Unquoted strings are not allowed");
                }
                UnquotedStringFallback = true;
                Position = TokenPosition;
                return ReadUnquotedString();
            }
        }
        UnquotedStringFallback = false;
        return new Token(this, TokenType, TokenPosition, Literal.Length);
    }
    private char ReadCharFromHexSequence() {
        Span<byte> HexUtf8Bytes = stackalloc byte[4];

        for (int Index = 0; Index < HexUtf8Bytes.Length; Index++) {
            // Peek rune
            if (ReadRune() is not Rune Rune) {
                throw new HjsonException("Expected hex digit in sequence, got end of stream");
            }

            // Hexadecimal rune
            if (Rune.Value is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                HexUtf8Bytes[Index] = (byte)Rune.Value;
            }
            // Unexpected rune
            else {
                throw new HjsonException($"Expected 4 hexadecimal digits for unicode escape sequence, got `{Rune}`");
            }
        }

        // Parse unicode character from 4 hexadecimal digits
        char UnicodeCharacter = (char)ushort.Parse(HexUtf8Bytes, NumberStyles.AllowHexSpecifier);
        return UnicodeCharacter;
    }
    private bool DetectFallbackToUnquotedString() {
        long StartTestPosition = Position;

        try {
            while (true) {
                // Peek rune
                if (PeekRune() is not Rune TestRune) {
                    return false;
                }

                // JSON symbol; unquoted strings cannot start with these
                if (TestRune.Value is ',' or '{' or '}' or '[' or ']' or ':' or '"' or '\'') {
                    return false;
                }
                // Comment
                else if (TestRune.Value is '#' or '/') {
                    ReadComment();
                }
                // Whitespace
                else if (Rune.IsWhiteSpace(TestRune)) {
                    ReadRune();
                }
                // Invalid rune; fallback to unquoted string
                else {
                    return true;
                }
            }
        }
        finally {
            Position = StartTestPosition;
        }
    }

    public readonly record struct Token(HjsonStream HjsonStream, JsonTokenType Type, long Position, long Length = 1, string Value = "") {
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