using System.Buffers;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LinkDotNet.StringBuilder;

namespace HjsonSharp;

/// <summary>
/// A reader that can read HJSON from a sequence of runes.
/// </summary>
public sealed class HjsonReader : RuneReader {
    /// <summary>
    /// The rune reader to read runes from.
    /// </summary>
    public RuneReader InnerRuneReader { get; set; }
    /// <summary>
    /// The options used by the stream including feature switches.
    /// </summary>
    public HjsonReaderOptions Options { get; set; }

    /// <summary>
    /// Constructs a stream that reads HJSON from a rune reader.
    /// </summary>
    public HjsonReader(RuneReader RuneReader, HjsonReaderOptions? Options = null) {
        InnerRuneReader = RuneReader;
        this.Options = Options ?? HjsonReaderOptions.Hjson;
    }
    /// <summary>
    /// Constructs a stream that reads HJSON from a byte stream.
    /// </summary>
    public HjsonReader(Stream Stream, Encoding? Encoding = null, HjsonReaderOptions? Options = null)
        : this(new StreamRuneReader(Stream, Encoding), Options) {
    }
    /// <summary>
    /// Constructs a stream that reads HJSON from a byte array.
    /// </summary>
    public HjsonReader(byte[] Bytes, Encoding? Encoding = null, HjsonReaderOptions? Options = null)
        : this(new MemoryStream(Bytes), Encoding, Options) {
    }
    /// <summary>
    /// Constructs a stream that reads HJSON from a string.
    /// </summary>
    public HjsonReader(string String, HjsonReaderOptions? Options = null)
        : this(new StringRuneReader(String), Options) {
    }
    /// <summary>
    /// Constructs a stream that reads HJSON from a list of runes.
    /// </summary>
    public HjsonReader(IList<Rune> List, HjsonReaderOptions? Options = null)
        : this(new ListRuneReader(List), Options) {
    }

    /// <summary>
    /// Parses a single root element from the stream.
    /// </summary>
    public static HjsonResult<T?> ParseElement<T>(Stream Stream, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(Stream, Encoding, Options);
        return HjsonReader.ParseElement<T>(IsRoot: true);
    }
    /// <inheritdoc cref="ParseElement{T}(Stream, Encoding?, HjsonReaderOptions?)"/>
    public static HjsonResult<JsonElement> ParseElement(Stream Stream, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(Stream, Encoding, Options);
    }
    /// <summary>
    /// Parses a single root element from the byte array.
    /// </summary>
    public static HjsonResult<T?> ParseElement<T>(byte[] Bytes, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(Bytes, Encoding, Options);
        return HjsonReader.ParseElement<T>(IsRoot: true);
    }
    /// <inheritdoc cref="ParseElement{T}(byte[], Encoding?, HjsonReaderOptions?)"/>
    public static HjsonResult<JsonElement> ParseElement(byte[] Bytes, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(Bytes, Encoding, Options);
    }
    /// <summary>
    /// Parses a single root element from the string.
    /// </summary>
    public static HjsonResult<T?> ParseElement<T>(string String, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(String, Options);
        return HjsonReader.ParseElement<T>(IsRoot: true);
    }
    /// <inheritdoc cref="ParseElement{T}(string, HjsonReaderOptions?)"/>
    public static HjsonResult<JsonElement> ParseElement(string String, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(String, Options);
    }
    /// <summary>
    /// Parses a single root element from the list of runes.
    /// </summary>
    public static HjsonResult<T?> ParseElement<T>(IList<Rune> List, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(List, Options);
        return HjsonReader.ParseElement<T>(IsRoot: true);
    }
    /// <inheritdoc cref="ParseElement{T}(IList{Rune}, HjsonReaderOptions?)"/>
    public static HjsonResult<JsonElement> ParseElement(IList<Rune> List, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(List, Options);
    }

    /// <inheritdoc/>
    public override long Position {
        get => InnerRuneReader.Position;
        set => InnerRuneReader.Position = value;
    }
    /// <inheritdoc/>
    public override long Length {
        get => InnerRuneReader.Length;
    }

    /// <inheritdoc/>
    public override Rune? Peek() {
        return InnerRuneReader.Peek();
    }
    /// <inheritdoc/>
    public override Rune? Read() {
        return InnerRuneReader.Read();
    }
    /// <inheritdoc/>
    public override bool TryRead(Rune? Expected) {
        return InnerRuneReader.TryRead(Expected);
    }
    /// <inheritdoc/>
    public override bool TryRead(char Expected) {
        return InnerRuneReader.TryRead(Expected);
    }
    /// <inheritdoc/>
    public override string ReadToEnd() {
        return InnerRuneReader.ReadToEnd();
    }
    /// <inheritdoc/>
    public override void Dispose() {
        base.Dispose();
        GC.SuppressFinalize(this);
        InnerRuneReader.Dispose();
    }

    /// <summary>
    /// Parses a single element from the stream.
    /// </summary>
    public HjsonResult<T?> ParseElement<T>(bool IsRoot) {
        return ParseNode(IsRoot).Try(Value => Value.Deserialize<T>(JsonOptions.Mini));
    }
    /// <inheritdoc cref="ParseElement{T}(bool)"/>
    public HjsonResult<JsonElement> ParseElement(bool IsRoot) {
        return ParseElement<JsonElement>(IsRoot);
    }
    /// <summary>
    /// Tries to parse a single element from the stream, returning <see langword="false"/> if an error occurs.
    /// </summary>
    public bool TryParseElement<T>(out T? Result, bool IsRoot) {
        if (ParseElement<T>(IsRoot).TryGetValue(out T? Value)) {
            Result = Value;
            return true;
        }
        else {
            Result = default;
            return false;
        }
    }
    /// <inheritdoc cref="TryParseElement{T}(out T, bool)"/>
    public bool TryParseElement(out JsonElement Result, bool IsRoot) {
        return TryParseElement<JsonElement>(out Result, IsRoot);
    }
    /// <summary>
    /// Parses a single <see cref="JsonNode"/> from the stream.
    /// </summary>
    public HjsonResult<JsonNode?> ParseNode(bool IsRoot) {
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
        void StartNode(JsonNode Node) {
            SubmitNode(Node);
            CurrentNode = Node;
        }

        foreach (HjsonResult<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token, out HjsonError Error)) {
                return Error;
            }

            // Null
            if (Token.Type is JsonTokenType.Null) {
                JsonValue? Node = null;
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // True
            else if (Token.Type is JsonTokenType.True) {
                JsonValue Node = JsonValue.Create(true);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // False
            else if (Token.Type is JsonTokenType.False) {
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
                // TODO:
                // A number node can't be created from a string yet, so create a string node instead.
                // See https://github.com/dotnet/runtime/discussions/111373
                JsonNode Node = JsonValue.Create(Token.Value);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // Start Object
            else if (Token.Type is JsonTokenType.StartObject) {
                JsonObject Node = [];
                StartNode(Node);
            }
            // Start Array
            else if (Token.Type is JsonTokenType.StartArray) {
                JsonArray Node = [];
                StartNode(Node);
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

        // End of input
        return new HjsonError("Expected token, got end of input");
    }
    /// <summary>
    /// Tries to parse a single <see cref="JsonNode"/> from the stream, returning <see langword="false"/> if an exception occurs.
    /// </summary>
    public bool TryParseNode(out JsonNode? Result, bool IsRoot) {
        if (ParseNode(IsRoot).TryGetValue(out JsonNode? Value)) {
            Result = Value;
            return true;
        }
        else {
            Result = default;
            return false;
        }
    }
    /// <summary>
    /// Reads the tokens of a single element from the stream.
    /// </summary>
    public IEnumerable<HjsonResult<Token>> ReadElement(bool IsRoot) {
        // Comments & whitespace
        foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
            yield return Token;
            if (Token.IsError) {
                yield break;
            }
        }

        // Root object with omitted root brackets
        if (IsRoot && Options.OmittedRootObjectBrackets && DetectObjectWithOmittedBrackets()) {
            foreach (HjsonResult<Token> Token in ReadObject(OmitBrackets: true)) {
                yield return Token;
                if (Token.IsError) {
                    yield break;
                }
            }
            yield break;
        }

        // Peek rune
        if (Peek() is not Rune Rune) {
            yield return new HjsonError("Expected token, got end of input");
            yield break;
        }

        // Object
        if (Rune.Value is '{') {
            foreach (HjsonResult<Token> Token in ReadObject(OmitBrackets: false)) {
                yield return Token;
                if (Token.IsError) {
                    yield break;
                }
            }
        }
        // Array
        else if (Rune.Value is '[') {
            foreach (HjsonResult<Token> Token in ReadArray()) {
                yield return Token;
                if (Token.IsError) {
                    yield break;
                }
            }
        }
        // Primitive
        else {
            yield return ReadPrimitiveElement();
        }
    }
    /// <summary>
    /// Tries to find the given property name in the stream.<br/>
    /// For example, to find <c>c</c>:
    /// <code>
    /// // Original position
    /// {
    ///   "a": "1",
    ///   "b": {
    ///     "c": "2"
    ///   },
    ///   "c":/* Final position */ "3"
    /// }
    /// </code>
    /// </summary>
    public bool FindPath(string PropertyName, bool IsRoot) {
        long CurrentDepth = 0;

        foreach (HjsonResult<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token)) {
                return false;
            }

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
    /// <summary>
    /// Tries to find the given array index in the stream.<br/>
    /// For example, to find <c>1</c>:
    /// <code>
    /// // Original position
    /// [
    ///   [
    ///     "a",
    ///     "b"
    ///   ],
    ///   /* Destination position */"c",
    ///   "d"
    /// ]
    /// </code>
    /// </summary>
    public bool FindPath(long ArrayIndex, bool IsRoot) {
        long CurrentDepth = 0;
        long CurrentIndex = -1;
        bool IsArray = false;

        foreach (HjsonResult<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token)) {
                return false;
            }

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

    private HjsonResult<Token> ReadPrimitiveElement() {
        // Peek rune
        if (Peek() is not Rune Rune) {
            return new HjsonError("Expected token, got end of input");
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
            return new HjsonError($"Invalid rune: `{Rune}`");
        }
    }
    private HjsonResult<Token> ReadNull() {
        // Null
        return ReadLiteralToken(JsonTokenType.Null, "null", out _);
    }
    private HjsonResult<Token> ReadBoolean() {
        // Peek rune
        if (Peek() is not Rune Rune) {
            return new HjsonError("Expected boolean, got end of input");
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
    private HjsonResult<Token> ReadString() {
        long TokenPosition = Position;

        // Opening quote
        Rune OpeningQuote;
        // Double-quoted string
        if (TryRead('"')) {
            OpeningQuote = new Rune('"');
        }
        else {
            if (TryRead('\'')) {
                if (TryRead('\'')) {
                    // Triple-quoted string
                    if (TryRead('\'')) {
                        if (!Options.TripleQuotedStrings) {
                            return new HjsonError("Triple-quoted strings are not allowed");
                        }
                        return ReadTripleQuotedString(new Rune('\''));
                    }
                    // Empty single-quoted string
                    else {
                        return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, "");
                    }
                }
                // Single-quoted string
                else {
                    if (!Options.SingleQuotedStrings) {
                        return new HjsonError("Single-quoted strings are not allowed");
                    }
                    OpeningQuote = new Rune('\'');
                }
            }
            // Unquoted string
            else {
                if (!Options.UnquotedStrings) {
                    return new HjsonError("Unquoted strings are not allowed");
                }
                return ReadUnquotedString();
            }
        }

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Read rune
            if (Read() is not Rune Rune) {
                return new HjsonError($"Expected `{OpeningQuote}` to end string, got end of input");
            }

            // Closing quote
            if (Rune == OpeningQuote) {
                break;
            }
            // Escape
            else if (Rune.Value is '\\') {
                // Read escaped rune
                if (Read() is not Rune EscapedRune) {
                    return new HjsonError("Expected escape character after `\\`, got end of input");
                }

                // Double quote
                if (EscapedRune.Value is '"') {
                    StringBuilder.Append('"');
                }
                // Single quote
                else if (EscapedRune.Value is '\'') {
                    if (!Options.SingleQuotedStrings && !Options.InvalidStringEscapeSequences) {
                        return new HjsonError("Escaped single quotes are not allowed");
                    }
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
                    if (!ReadRuneFromHexSequence(4).TryGetValue(out Rune ResultRune, out HjsonError Error)) {
                        return Error;
                    }
                    StringBuilder.Append(ResultRune);
                }
                // Unicode short hex sequence
                else if (EscapedRune.Value is 'x') {
                    if (!Options.EscapedStringShortHexSequences && !Options.InvalidStringEscapeSequences) {
                        return new HjsonError("Escaped short hex sequences are not allowed");
                    }
                    if (!ReadRuneFromHexSequence(2).TryGetValue(out Rune ResultRune, out HjsonError Error)) {
                        return Error;
                    }
                    StringBuilder.Append(ResultRune);
                }
                // Newline
                else if (EscapedRune.Value is '\n' or '\r' or '\u2028' or '\u2029') {
                    if (!Options.EscapedStringNewlines && !Options.InvalidStringEscapeSequences) {
                        return new HjsonError("Escaped newlines are not allowed");
                    }
                    // Join CR LF
                    if (EscapedRune.Value is '\r') {
                        TryRead('\n');
                    }
                }
                // Invalid escape character
                else {
                    if (!Options.InvalidStringEscapeSequences) {
                        return new HjsonError($"Expected valid escape character after `\\`, got `{EscapedRune}`");
                    }
                    StringBuilder.Append(EscapedRune);
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
            if (Read() is not Rune Rune) {
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
    private HjsonResult<Token> ReadTripleQuotedString(Rune OpeningQuote, int OpeningQuoteCount = 3) {
        long TokenPosition = Position;

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        int ClosingQuoteCounter = 0;
        int LeadingWhitespaceCounter = 0;
        bool IsLeadingWhitespace = false;
        bool IsFirstLine = true;

        while (true) {
            // Read rune
            if (Read() is not Rune Rune) {
                return new HjsonError($"Expected `{string.Concat(Enumerable.Repeat(OpeningQuote, OpeningQuoteCount))}` to end string, got end of input");
            }

            // Closing quote
            if (Rune == OpeningQuote) {
                ClosingQuoteCounter++;
                if (ClosingQuoteCounter == OpeningQuoteCount) {
                    break;
                }
            }
            // Newline
            else if (Rune.Value is '\n' or '\r') {
                // Join CR LF
                if (Rune.Value is '\r') {
                    TryRead('\n');
                }

                // Start of leading whitespace
                LeadingWhitespaceCounter = 0;
                IsLeadingWhitespace = true;

                // Skip whitespace on first line
                if (IsFirstLine) {
                    IsFirstLine = false;
                    continue;
                }

                StringBuilder.Append(Rune);
            }
            // Whitespace
            else if (Rune.IsWhiteSpace(Rune)) {
                // Build leading whitespace
                if (IsLeadingWhitespace) {
                    LeadingWhitespaceCounter++;
                }

                // Skip whitespace on first line
                if (IsFirstLine && IsLeadingWhitespace) {
                    continue;
                }

                StringBuilder.Append(Rune);
            }
            // Rune
            else {
                // Reset closing triple-quote counter
                ClosingQuoteCounter = 0;
                // End of leading whitespace
                IsLeadingWhitespace = false;

                StringBuilder.Append(Rune);
            }
        }

        // Trim leading whitespace preceding closing quotes
        if (LeadingWhitespaceCounter > 0) {
            int TrimLeadingWhitespaceCounter = 0;
            int StartLeadingWhitespaceIndex = 0;
            bool IsInLeadingWhitespace = true;
            for (int Index = 0; Index < StringBuilder.Length;) {
                // Get current rune
                if (Rune.DecodeFromUtf16(StringBuilder.AsSpan()[Index..], out Rune CurrentRune, out int CurrentRuneLength) is not OperationStatus.Done) {
                    throw new InvalidProgramException("Could not decode rune previously built when reading triple quoted string");
                }

                // Start of leading whitespace
                if (CurrentRune.Value is '\n' or '\r') {
                    // Join CR LF
                    if (CurrentRune.Value is '\r') {
                        TryRead('\n');
                    }

                    // Remove leading whitespace
                    if (IsInLeadingWhitespace) {
                        StringBuilder.Remove(StartLeadingWhitespaceIndex, Index - StartLeadingWhitespaceIndex);
                        Index = StartLeadingWhitespaceIndex;
                    }

                    // Reset leading whitespace
                    TrimLeadingWhitespaceCounter = 0;
                    StartLeadingWhitespaceIndex = Index + CurrentRuneLength;
                    IsInLeadingWhitespace = true;
                }
                // Whitespace
                else if (Rune.IsWhiteSpace(CurrentRune)) {
                    if (IsInLeadingWhitespace) {
                        // Build leading whitespace
                        TrimLeadingWhitespaceCounter++;

                        // Remove leading whitespace when end reached
                        if (TrimLeadingWhitespaceCounter > LeadingWhitespaceCounter) {
                            IsInLeadingWhitespace = false;

                            // Remove leading whitespace
                            StringBuilder.Remove(StartLeadingWhitespaceIndex, Index - StartLeadingWhitespaceIndex);
                            Index = StartLeadingWhitespaceIndex;
                        }
                    }
                }
                // End of leading whitespace
                else if (IsInLeadingWhitespace) {
                    IsInLeadingWhitespace = false;

                    // Remove leading whitespace
                    StringBuilder.Remove(StartLeadingWhitespaceIndex, Index - StartLeadingWhitespaceIndex);
                    Index = StartLeadingWhitespaceIndex;
                }

                // Move index to next rune
                Index += CurrentRuneLength;
            }
            // Remove leading whitespace on last line
            StringBuilder.Remove(StringBuilder.Length - LeadingWhitespaceCounter, LeadingWhitespaceCounter);
            // Remove last newline
            if (StringBuilder.Length >= 1 && StringBuilder[^1] is '\n' or '\r') {
                StringBuilder.Remove(StringBuilder.Length - 1, 1);
            }
        }

        // End token
        return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private HjsonResult<Token> ReadNumber() {
        long TokenPosition = Position;

        HjsonResult<Token> ReadNumberNoFallback() {
            // Create string builder
            ValueStringBuilder StringBuilder = new();

            bool ParsedExponent = false;
            bool ParsedDecimalPoint = false;
            bool ParsedNonZeroDigit = false;
            bool TrailingExponent = false;
            bool TrailingDecimalPoint = false;
            bool TrailingSign = false;
            bool LeadingZero = false;
            bool IsHexadecimal = false;

            // Sign
            if (TryRead('-')) {
                StringBuilder.Append('-');
                TrailingSign = true;
            }
            else if (TryRead('+')) {
                if (!Options.ExplicitPlusSigns) {
                    return new HjsonError("Explicit plus-signs are not allowed");
                }
                StringBuilder.Append('+');
                TrailingSign = true;
            }

            // Named floating point literal
            if (Options.NamedFloatingPointLiterals) {
                if (Peek() is Rune LiteralRune && LiteralRune.Value is 'I' or 'N') {
                    // Guess full literal
                    string Literal = LiteralRune.Value is 'I' ? "Infinity" : "NaN";
                    // Read full literal
                    if (!ReadLiteralToken(JsonTokenType.String, Literal, out bool UnquotedStringFallback).TryGetValue(out Token LiteralToken, out HjsonError Error)) {
                        return Error;
                    }

                    // Unquoted string was read
                    if (UnquotedStringFallback) {
                        StringBuilder.Append(LiteralToken.Value);
                    }
                    // Full literal was read
                    else {
                        StringBuilder.Append(Literal);
                    }

                    // Submit string token
                    return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
                }
            }

            // Leading decimal point
            if (TryRead('.')) {
                if (!Options.LeadingDecimalPoints) {
                    return new HjsonError("Leading decimal points are not allowed");
                }

                TrailingSign = false;
                TrailingDecimalPoint = true;

                StringBuilder.Append('.');
            }

            while (true) {
                // Peek rune
                Rune? Rune = Peek();

                // Digit
                if (Rune?.Value is >= '0' and <= '9') {
                    if (Rune.Value.Value is '0') {
                        if (!ParsedNonZeroDigit) {
                            LeadingZero = true;
                        }
                    }
                    else {
                        ParsedNonZeroDigit = true;
                    }

                    TrailingExponent = false;
                    TrailingDecimalPoint = false;
                    TrailingSign = false;

                    Read();
                    StringBuilder.Append(Rune.Value);
                }
                // Hexadecimal digit
                else if (IsHexadecimal && Rune?.Value is (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                    ParsedNonZeroDigit = true;

                    TrailingExponent = false;
                    TrailingDecimalPoint = false;
                    TrailingSign = false;

                    Read();
                    StringBuilder.Append(Rune.Value);
                }
                // Exponent
                else if (Rune?.Value is 'e' or 'E') {
                    if (ParsedExponent) {
                        return new HjsonError($"Duplicate exponent: `{Rune}`");
                    }
                    if (TrailingSign) {
                        return new HjsonError($"Expected digit before exponent: `{Rune}`");
                    }

                    ParsedExponent = true;

                    TrailingExponent = true;
                    TrailingDecimalPoint = false;

                    Read();
                    StringBuilder.Append(Rune.Value);

                    // Exponent sign
                    if (TryRead('-')) {
                        StringBuilder.Append('-');
                    }
                    else if (TryRead('+')) {
                        StringBuilder.Append('+');
                    }
                }
                // Decimal point
                else if (Rune?.Value is '.') {
                    if (ParsedDecimalPoint) {
                        return new HjsonError($"Duplicate decimal point: `{Rune}`");
                    }
                    if (ParsedExponent) {
                        return new HjsonError($"Exponent cannot be fractional: `{Rune}`");
                    }

                    ParsedDecimalPoint = true;

                    TrailingExponent = false;
                    TrailingDecimalPoint = true;
                    TrailingSign = false;

                    Read();
                    StringBuilder.Append(Rune.Value);
                }
                // Hexadecimal specifier
                else if (Rune?.Value is 'x' or 'X') {
                    if (ParsedNonZeroDigit || ParsedDecimalPoint || ParsedExponent) {
                        return new HjsonError($"Hexadecimal specifier must be at the start of the number: `{Rune}`");
                    }
                    if (!Options.HexadecimalNumbers) {
                        return new HjsonError("Hexadecimal numbers are not allowed");
                    }

                    IsHexadecimal = true;

                    LeadingZero = false;
                    ParsedNonZeroDigit = true;

                    TrailingExponent = false;
                    TrailingDecimalPoint = true;
                    TrailingSign = false;

                    Read();
                    StringBuilder.Append(Rune.Value);
                }
                // End of number
                else {
                    if (!Options.TrailingDecimalPoints) {
                        if (TrailingDecimalPoint) {
                            return new HjsonError("Expected digit after decimal point");
                        }
                    }
                    if (TrailingExponent) {
                        return new HjsonError("Expected digit after exponent");
                    }
                    if (TrailingSign) {
                        return new HjsonError("Expected digit after sign");
                    }
                    if (!Options.LeadingZeroes) {
                        if (LeadingZero && ParsedNonZeroDigit) {
                            return new HjsonError("Leading zeroes are not allowed");
                        }
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

        HjsonResult<Token> NumberToken = ReadNumberNoFallback();

        // Fallback to unquoted string
        if (NumberToken.IsError && Options.UnquotedStrings) {
            Position = TokenPosition;
            return ReadUnquotedString();
        }

        return NumberToken;
    }
    private IEnumerable<HjsonResult<Token>> ReadObject(bool OmitBrackets) {
        // Opening bracket
        if (!OmitBrackets) {
            if (!TryRead('{')) {
                yield return new HjsonError($"Expected `{{` to start object");
                yield break;
            }
            yield return new Token(this, JsonTokenType.StartObject, Position - 1);
        }
        // Start of object with omitted brackets
        else {
            yield return new Token(this, JsonTokenType.StartObject, Position);
        }

        // Comments & whitespace
        foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
            yield return Token;
            if (Token.IsError) {
                yield break;
            }
        }

        bool PropertyLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                // Missing closing bracket
                if (!OmitBrackets) {
                    yield return new HjsonError("Expected `}` to end object, got end of input");
                    yield break;
                }
                // End of object with omitted brackets
                yield return new Token(this, JsonTokenType.EndObject, Position);
                yield break;
            }

            // Closing bracket
            if (Rune.Value is '}') {
                // Unexpected closing bracket in object with omitted brackets
                if (OmitBrackets) {
                    yield return new HjsonError("Unexpected `}` in object with omitted brackets");
                    yield break;
                }
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        yield return new HjsonError("Trailing commas are not allowed");
                        yield break;
                    }
                }
                // End of object
                yield return new Token(this, JsonTokenType.EndObject, Position);
                Read();
                yield break;
            }
            // Property name
            else {
                // Unexpected property name
                if (!PropertyLegal) {
                    yield return new HjsonError("Expected `,` before property name in object");
                    yield break;
                }

                // Property name
                foreach (HjsonResult<Token> Token in ReadPropertyName()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Comments & whitespace
                foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Property value
                foreach (HjsonResult<Token> Token in ReadElement(IsRoot: false)) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Comments & whitespace
                foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Comma
                TrailingComma = TryRead(',');
                PropertyLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }
            }
        }
    }
    private IEnumerable<HjsonResult<Token>> ReadPropertyName() {
        long TokenPosition = Position;

        // Unquoted property name
        if (Peek()?.Value is not ('"' or '\'')) {
            // ECMAScript property name
            if (Options.EcmaScriptPropertyNames) {
                foreach (HjsonResult<Token> Token in ReadEcmaScriptPropertyName()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }
                yield break;
            }
            // Unquoted property name
            else if (Options.UnquotedPropertyNames) {
                yield return ReadUnquotedPropertyName();
                yield break;
            }
            else {
                yield return new HjsonError("Unquoted property names are not allowed");
            }
        }

        // String
        if (!ReadString().TryGetValue(out Token String, out HjsonError StringError)) {
            yield return StringError;
            yield break;
        }

        // Comments & whitespace
        foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
            yield return Token;
            if (Token.IsError) {
                yield break;
            }
        }

        // Colon
        if (!TryRead(':')) {
            yield return new HjsonError($"Expected `:` after property name in object");
            yield break;
        }
        yield return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, String.Value);
    }
    private IEnumerable<HjsonResult<Token>> ReadEcmaScriptPropertyName() {
        long TokenPosition = Position;

        // Start token
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return [new HjsonError($"Expected `:` after property name in object")];
            }

            // Colon
            if (Rune.Value is ':') {
                Read();
                break;
            }
            // Comments & whitespace
            else if (Rune.Value is '#' or '/' || Rune.IsWhiteSpace(Rune)) {
                // A local iterator function is used here to prevent compiler errors for ValueStringBuilder, which is a ref struct
                IEnumerable<HjsonResult<Token>> ReadCommentsAndWhitespaceThenColon() {
                    // Comments & whitespace
                    foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                        yield return Token;
                        if (Token.IsError) {
                            yield break;
                        }
                    }

                    // Colon
                    if (!TryRead(':')) {
                        yield return new HjsonError($"Expected `:` after property name in object");
                        yield break;
                    }
                }
                // End token
                return ReadCommentsAndWhitespaceThenColon()
                    .Append(new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString()));
            }
            // Dollar sign
            else if (Rune.Value is '$') {
                Read();
                StringBuilder.Append('$');
            }
            // Underscore
            else if (Rune.Value is '_') {
                Read();
                StringBuilder.Append('_');
            }
            // Escape
            else if (Rune.Value is '\\') {
                Read();
                // Read escaped rune
                if (Read() is not Rune EscapedRune) {
                    return [new HjsonError("Expected escape character after `\\`, got end of input")];
                }

                // Unicode hex sequence
                if (EscapedRune.Value is 'u') {
                    if (!ReadRuneFromHexSequence(4).TryGetValue(out Rune ResultRune, out HjsonError Error)) {
                        return [Error];
                    }
                    StringBuilder.Append(ResultRune);
                }
                // Invalid escape character
                else {
                    return [new HjsonError($"Expected valid escape character after `\\`, got `{EscapedRune}`")];
                }
            }
            // Unicode letter
            else if (Rune.IsLetter(Rune)) {
                Read();
                StringBuilder.Append(Rune);
            }
            // Invalid rune
            else {
                return [new HjsonError($"Unexpected rune in property name: `{Rune}`")];
            }
        }

        // End token
        return [new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString())];
    }
    private HjsonResult<Token> ReadUnquotedPropertyName() {
        long TokenPosition = Position;

        // Start token
        ValueStringBuilder StringBuilder = new();

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return new HjsonError($"Expected `:` after property name in object");
            }

            // Colon
            if (Rune.Value is ':') {
                Read();
                break;
            }
            // Invalid rune
            else if (Rune.Value is ',' or ':' or '[' or ']' or '{' or '}' || Rune.IsWhiteSpace(Rune)) {
                return new HjsonError($"Unexpected rune in property name: `{Rune}`");
            }
            // Valid rune
            else {
                Read();
                StringBuilder.Append(Rune);
            }
        }

        // End token
        return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private IEnumerable<HjsonResult<Token>> ReadArray() {
        // Opening bracket
        if (!TryRead('[')) {
            yield return new HjsonError($"Expected `[` to start array");
            yield break;
        }
        yield return new Token(this, JsonTokenType.StartArray, Position - 1);

        // Comments & whitespace
        foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
            yield return Token;
            if (Token.IsError) {
                yield break;
            }
        }

        bool ItemLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                yield return new HjsonError("Expected `]` to end array, got end of input");
                yield break;
            }

            // Closing bracket
            if (Rune.Value is ']') {
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        yield return new HjsonError("Trailing commas are not allowed");
                        yield break;
                    }
                }
                // End of array
                yield return new Token(this, JsonTokenType.EndArray, Position);
                Read();
                yield break;
            }
            // Item
            else {
                // Unexpected item
                if (!ItemLegal) {
                    yield return new HjsonError("Expected `,` before item in array");
                    yield break;
                }

                // Item
                foreach (HjsonResult<Token> Token in ReadElement(IsRoot: false)) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Comments & whitespace
                foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }

                // Comma
                TrailingComma = TryRead(',');
                ItemLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                    yield return Token;
                    if (Token.IsError) {
                        yield break;
                    }
                }
            }
        }
    }
    private IEnumerable<HjsonResult<Token>> ReadCommentsAndWhitespace() {
        while (true) {
            // Whitespace
            if (ReadWhitespace().TryGetError(out HjsonError WhitespaceError)) {
                yield return WhitespaceError;
                yield break;
            }

            // Peek rune
            if (Peek() is not Rune Rune) {
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
    private HjsonResult<Token> ReadComment() {
        long TokenPosition = Position;

        // Comment type
        bool IsBlockComment = false;
        if (TryRead('/')) {
            // Line-style comment
            if (TryRead('/')) {
                // Ensure line-style comments are enabled
                if (!Options.LineStyleComments) {
                    return new HjsonError("Line-style comments are not allowed");
                }
            }
            // Block-style comment
            else if (TryRead('*')) {
                // Ensure block-style comments are enabled
                if (!Options.BlockStyleComments) {
                    return new HjsonError("Block-style comments are not allowed");
                }
                IsBlockComment = true;
            }
        }
        // Hash-style comment
        else if (TryRead('#')) {
            // Ensure hash-style comments are enabled
            if (!Options.HashStyleComments) {
                return new HjsonError("Hash-style comments are not allowed");
            }
        }
        // Invalid comment
        else {
            return new HjsonError($"Expected `#` or `//` or `/*` to start comment");
        }

        // Create string builder
        ValueStringBuilder StringBuilder = new();

        // Read comment
        while (true) {
            // Read rune
            if (Read() is not Rune CommentRune) {
                if (IsBlockComment) {
                    return new HjsonError("Expected `*/` to end block-style comment, got end of input");
                }
                break;
            }

            // Check end of block comment
            if (IsBlockComment) {
                if (CommentRune.Value is '*' && Peek()?.Value is '/') {
                    Read();
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
    private HjsonResult ReadWhitespace() {
        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return HjsonResult.Success;
            }

            // JSON whitespace
            if (Rune.Value is '\n' or '\r' or ' ' or '\t') {
                Read();
            }
            // All whitespace
            else if (Rune.IsWhiteSpace(Rune)) {
                if (!Options.AllWhitespace) {
                    return new HjsonError("Non-JSON whitespace is not allowed");
                }
                Read();
            }
            // End of whitespace
            else {
                return HjsonResult.Success;
            }
        }
    }
    private HjsonResult<Token> ReadLiteralToken(JsonTokenType TokenType, scoped ReadOnlySpan<char> Literal, out bool UnquotedStringFallback) {
        long TokenPosition = Position;

        // Literal
        foreach (Rune ExpectedRune in Literal.EnumerateRunes()) {
            // Read rune
            Rune? ActualRune = Read();

            // Expected rune
            if (ActualRune == ExpectedRune) {
                continue;
            }
            // Unquoted string
            else {
                UnquotedStringFallback = true;
                if (!Options.UnquotedStrings) {
                    return new HjsonError("Unquoted strings are not allowed");
                }
                Position = TokenPosition;
                return ReadUnquotedString();
            }
        }
        UnquotedStringFallback = false;
        return new Token(this, TokenType, TokenPosition, Literal.Length);
    }
    private HjsonResult<Rune> ReadRuneFromHexSequence(int Length) {
        Span<byte> HexUtf8Bytes = stackalloc byte[Length];

        for (int Index = 0; Index < Length; Index++) {
            // Peek rune
            if (Read() is not Rune Rune) {
                return new HjsonError("Expected hex digit in sequence, got end of input");
            }

            // Hexadecimal rune
            if (Rune.Value is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                HexUtf8Bytes[Index] = (byte)Rune.Value;
            }
            // Unexpected rune
            else {
                return new HjsonError($"Expected {Length} hexadecimal digits for unicode escape sequence, got `{Rune}`");
            }
        }

        // Parse unicode character from hexadecimal digits
        return new Rune(int.Parse(HexUtf8Bytes, NumberStyles.AllowHexSpecifier));
    }
    private bool DetectFallbackToUnquotedString() {
        long StartTestPosition = Position;
        try {
            while (true) {
                // Peek rune
                if (Peek() is not Rune TestRune) {
                    return false;
                }

                // JSON symbol; unquoted strings cannot start with these
                if (TestRune.Value is ',' or ':' or '{' or '}' or '[' or ']' or '"' or '\'') {
                    return false;
                }
                // Comment
                else if (TestRune.Value is '#' or '/') {
                    ReadComment();
                }
                // Whitespace
                else if (Rune.IsWhiteSpace(TestRune)) {
                    Read();
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
    private bool DetectObjectWithOmittedBrackets() {
        long StartTestPosition = Position;
        try {
            // Comments & whitespace
            foreach (HjsonResult<Token> Token in ReadCommentsAndWhitespace()) {
                if (Token.IsError) {
                    return false;
                }
            }

            // Property name (including colon)
            foreach (HjsonResult<Token> Token in ReadPropertyName()) {
                if (Token.IsError) {
                    return false;
                }
            }

            // We read a property name (e.g. `a:`), so must be an object with omitted brackets
            return true;
        }
        finally {
            Position = StartTestPosition;
        }
    }

    /// <summary>
    /// A single token for a <see cref="JsonTokenType"/> in a <see cref="HjsonReader"/>.
    /// </summary>
    public readonly record struct Token(HjsonReader HjsonReader, JsonTokenType Type, long Position, long Length = 1, string Value = "") {
        /// <summary>
        /// Parses a single element at the token's position in the <see cref="HjsonReader"/>.
        /// </summary>
        public HjsonResult<T?> ParseElement<T>(bool IsRoot) {
            // Go to token position
            long OriginalPosition = HjsonReader.Position;
            HjsonReader.Position = Position;
            try {
                // Parse element
                return HjsonReader.ParseElement<T>(IsRoot);
            }
            finally {
                // Return to original position
                HjsonReader.Position = OriginalPosition;
            }
        }
        /// <inheritdoc cref="ParseElement{T}(bool)"/>
        public HjsonResult<JsonElement> ParseElement(bool IsRoot) {
            return ParseElement<JsonElement>(IsRoot);
        }
    }
}