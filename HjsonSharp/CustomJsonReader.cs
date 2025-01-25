using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using LinkDotNet.StringBuilder;
using ResultZero;

namespace HjsonSharp;

/// <summary>
/// A reader that can read custom JSON from a sequence of runes.
/// </summary>
public sealed class CustomJsonReader : RuneReader {
    /// <summary>
    /// The rune reader to read runes from.
    /// </summary>
    public RuneReader InnerRuneReader { get; set; }
    /// <summary>
    /// The options used by the reader including feature switches.
    /// </summary>
    public CustomJsonReaderOptions Options { get; set; }

    private static readonly Rune[] NewlineRunes = [(Rune)'\n', (Rune)'\r', (Rune)'\u2028', (Rune)'\u2029'];
    private static readonly char[] NewlineChars = ['\n', '\r', '\u2028', '\u2029'];

    /// <summary>
    /// Constructs a reader that reads JSON from a rune reader.
    /// </summary>
    public CustomJsonReader(RuneReader RuneReader, CustomJsonReaderOptions? Options = null) {
        InnerRuneReader = RuneReader;
        this.Options = Options ?? CustomJsonReaderOptions.Json;
    }
    /// <summary>
    /// Constructs a reader that reads custom JSON from a byte stream.
    /// </summary>
    public CustomJsonReader(Stream Stream, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null)
        : this(new StreamRuneReader(Stream, Encoding), Options) {
    }
    /// <summary>
    /// Constructs a reader that reads custom JSON from a byte array.
    /// </summary>
    public CustomJsonReader(byte[] Bytes, int Index, int Count, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null)
        : this(new MemoryStream(Bytes, Index, Count), Encoding, Options) {
    }
    /// <inheritdoc cref="CustomJsonReader(byte[], int, int, Encoding?, CustomJsonReaderOptions?)"/>
    public CustomJsonReader(byte[] Bytes, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null)
        : this(new MemoryStream(Bytes), Encoding, Options) {
    }
    /// <summary>
    /// Constructs a reader that reads custom JSON from a string.
    /// </summary>
    public CustomJsonReader(string String, int Index, int Count, CustomJsonReaderOptions? Options = null)
        : this(new StringRuneReader(String, Index, Count), Options) {
    }
    /// <inheritdoc cref="CustomJsonReader(string, int, int, CustomJsonReaderOptions?)"/>
    public CustomJsonReader(string String, CustomJsonReaderOptions? Options = null)
        : this(new StringRuneReader(String), Options) {
    }
    /// <summary>
    /// Constructs a reader that reads custom JSON from a list of runes.
    /// </summary>
    public CustomJsonReader(IList<Rune> List, CustomJsonReaderOptions? Options = null)
        : this(new ListRuneReader(List), Options) {
    }

    /// <summary>
    /// Parses a single element from the byte stream.
    /// </summary>
    public static Result<T?> ParseElement<T>(Stream Stream, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        using CustomJsonReader Reader = new(Stream, Encoding, Options);
        return Reader.ParseElement<T>(IsRoot);
    }
    /// <inheritdoc cref="ParseElement{T}(Stream, Encoding?, CustomJsonReaderOptions?, bool)"/>
    public static Result<JsonElement> ParseElement(Stream Stream, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        return ParseElement<JsonElement>(Stream, Encoding, Options, IsRoot);
    }
    /// <summary>
    /// Parses a single element from the byte array.
    /// </summary>
    public static Result<T?> ParseElement<T>(byte[] Bytes, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        using CustomJsonReader Reader = new(Bytes, Encoding, Options);
        return Reader.ParseElement<T>(IsRoot);
    }
    /// <inheritdoc cref="ParseElement{T}(byte[], Encoding?, CustomJsonReaderOptions?, bool)"/>
    public static Result<JsonElement> ParseElement(byte[] Bytes, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        return ParseElement<JsonElement>(Bytes, Encoding, Options, IsRoot);
    }
    /// <inheritdoc cref="ParseElement(byte[], Encoding?, CustomJsonReaderOptions?, bool)"/>
    public static Result<T?> ParseElement<T>(byte[] Bytes, int Index, int Count, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        using CustomJsonReader Reader = new(Bytes, Index, Count, Encoding, Options);
        return Reader.ParseElement<T>(IsRoot);
    }
    /// <inheritdoc cref="ParseElement(byte[], int, int, Encoding?, CustomJsonReaderOptions?, bool)"/>
    public static Result<JsonElement> ParseElement(byte[] Bytes, int Index, int Count, Encoding? Encoding = null, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        return ParseElement<JsonElement>(Bytes, Index, Count, Encoding, Options, IsRoot);
    }
    /// <summary>
    /// Parses a single element from the string.
    /// </summary>
    public static Result<T?> ParseElement<T>(string String, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        using CustomJsonReader Reader = new(String, Options);
        return Reader.ParseElement<T>(IsRoot);
    }
    /// <inheritdoc cref="ParseElement{T}(string, CustomJsonReaderOptions?, bool)"/>
    public static Result<JsonElement> ParseElement(string String, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        return ParseElement<JsonElement>(String, Options, IsRoot);
    }
    /// <inheritdoc cref="ParseElement{T}(string, CustomJsonReaderOptions?, bool)"/>
    public static Result<T?> ParseElement<T>(string String, int Index, int Count, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        using CustomJsonReader Reader = new(String, Index, Count, Options);
        return Reader.ParseElement<T>(IsRoot);
    }
    /// <inheritdoc cref="ParseElement(string, int, int, CustomJsonReaderOptions?, bool)"/>
    public static Result<JsonElement> ParseElement(string String, int Index, int Count, CustomJsonReaderOptions? Options = null, bool IsRoot = true) {
        return ParseElement<JsonElement>(String, Index, Count, Options, IsRoot);
    }
    /// <summary>
    /// Parses a single element from the list of runes.
    /// </summary>
    public static Result<T?> ParseElement<T>(IList<Rune> List, CustomJsonReaderOptions? Options = null) {
        using CustomJsonReader Reader = new(List, Options);
        return Reader.ParseElement<T>();
    }
    /// <inheritdoc cref="ParseElement{T}(IList{Rune}, CustomJsonReaderOptions?)"/>
    public static Result<JsonElement> ParseElement(IList<Rune> List, CustomJsonReaderOptions? Options = null) {
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
    /// Parses a single element from the reader.
    /// </summary>
    public Result<T?> ParseElement<T>(bool IsRoot = true) {
        return ParseNode(IsRoot).Try(Value => Value.Deserialize<T>(GlobalJsonOptions.Mini));
    }
    /// <inheritdoc cref="ParseElement{T}(bool)"/>
    public Result<JsonElement> ParseElement(bool IsRoot = true) {
        return ParseElement<JsonElement>(IsRoot);
    }
    /// <summary>
    /// Tries to parse a single element from the reader, returning <see langword="false"/> if an error occurs.
    /// </summary>
    public bool TryParseElement<T>(out T? Result, bool IsRoot = true) {
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
    public bool TryParseElement(out JsonElement Result, bool IsRoot = true) {
        return TryParseElement<JsonElement>(out Result, IsRoot);
    }
    /// <summary>
    /// Parses a single <see cref="JsonNode"/> from the reader.
    /// </summary>
    public Result<JsonNode?> ParseNode(bool IsRoot = true) {
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

        foreach (Result<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token, out Error Error)) {
                return Error;
            }

            // Null
            if (Token.JsonType is JsonTokenType.Null) {
                JsonValue? Node = null;
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // True
            else if (Token.JsonType is JsonTokenType.True) {
                JsonValue Node = JsonValue.Create(true);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // False
            else if (Token.JsonType is JsonTokenType.False) {
                JsonValue Node = JsonValue.Create(false);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // String
            else if (Token.JsonType is JsonTokenType.String) {
                JsonValue Node = JsonValue.Create(Token.Value);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // Number
            else if (Token.JsonType is JsonTokenType.Number) {
                // TODO:
                // A number node can't be created from a string yet, so create a string node instead.
                // See https://github.com/dotnet/runtime/discussions/111373
                JsonNode Node = JsonValue.Create(Token.Value);
                if (SubmitNode(Node)) {
                    return Node;
                }
            }
            // Start Object
            else if (Token.JsonType is JsonTokenType.StartObject) {
                JsonObject Node = [];
                StartNode(Node);
            }
            // Start Array
            else if (Token.JsonType is JsonTokenType.StartArray) {
                JsonArray Node = [];
                StartNode(Node);
            }
            // End Object/Array
            else if (Token.JsonType is JsonTokenType.EndObject or JsonTokenType.EndArray) {
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
            else if (Token.JsonType is JsonTokenType.PropertyName) {
                CurrentPropertyName = Token.Value;
            }
            // Comment
            else if (Token.JsonType is JsonTokenType.Comment) {
                // Pass
            }
            // Not implemented
            else {
                throw new NotImplementedException(Token.JsonType.ToString());
            }
        }

        // End of input
        return new Error("Expected token, got end of input");
    }
    /// <summary>
    /// Tries to parse a single <see cref="JsonNode"/> from the reader, returning <see langword="false"/> if an exception occurs.
    /// </summary>
    public bool TryParseNode(out JsonNode? Result, bool IsRoot = true) {
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
    /// Reads the tokens of a single element from the reader.
    /// </summary>
    public IEnumerable<Result<Token>> ReadElement(bool IsRoot = true) {
        // Comments & whitespace
        foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
            if (Token.IsError) {
                yield return Token.Error;
                yield break;
            }
            yield return Token;
        }

        // Root object with omitted root braces
        if (IsRoot && Options.OmittedObjectBraces && DetectObjectWithOmittedBraces()) {
            foreach (Result<Token> Token in ReadObject(OmitBraces: true, IsRoot)) {
                if (Token.IsError) {
                    yield return Token.Error;
                    yield break;
                }
                yield return Token;
            }
            yield break;
        }

        // Peek rune
        if (Peek() is not Rune Rune) {
            yield return new Error("Expected token, got end of input");
            yield break;
        }

        // Object
        if (Rune.Value is '{') {
            foreach (Result<Token> Token in ReadObject(OmitBraces: false, IsRoot)) {
                if (Token.IsError) {
                    yield return Token.Error;
                    yield break;
                }
                yield return Token;
            }
        }
        // Array
        else if (Rune.Value is '[') {
            foreach (Result<Token> Token in ReadArray(IsRoot)) {
                if (Token.IsError) {
                    yield return Token.Error;
                    yield break;
                }
                yield return Token;
            }
        }
        // Primitive
        else {
            yield return ReadPrimitiveElement();
        }
    }
    /// <summary>
    /// Reads the tokens of a single element from the reader and returns the length according to <see cref="Position"/>.
    /// </summary>
    public Result<long> ReadElementLength(bool IsRoot = true) {
        long OriginalPosition = Position;
        foreach (Result<Token> Token in ReadElement(IsRoot)) {
            if (Token.IsError) {
                return Token.Error;
            }
        }
        return Position - OriginalPosition;
    }
    /// <summary>
    /// Tries to find the given property name in the reader.<br/>
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
    public bool FindPropertyValue(string PropertyName, bool IsRoot = true) {
        long CurrentDepth = 0;

        foreach (Result<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token)) {
                return false;
            }

            // Start structure
            if (Token.JsonType is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                CurrentDepth++;
            }
            // End structure
            else if (Token.JsonType is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                CurrentDepth--;
            }
            // Property name
            else if (Token.JsonType is JsonTokenType.PropertyName) {
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
    /// Tries to find the given array index in the reader.<br/>
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
    public bool FindArrayIndex(long ArrayIndex, bool IsRoot = true) {
        long CurrentDepth = 0;
        long CurrentIndex = -1;
        bool IsArray = false;

        foreach (Result<Token> TokenResult in ReadElement(IsRoot)) {
            // Check error
            if (!TokenResult.TryGetValue(out Token Token)) {
                return false;
            }

            // Start structure
            if (Token.JsonType is JsonTokenType.StartObject or JsonTokenType.StartArray) {
                CurrentDepth++;
                if (CurrentDepth == 1) {
                    IsArray = Token.JsonType is JsonTokenType.StartArray;
                }
            }
            // End structure
            else if (Token.JsonType is JsonTokenType.EndObject or JsonTokenType.EndArray) {
                CurrentDepth--;
            }
            // Primitive value
            else if (Token.JsonType is JsonTokenType.Null or JsonTokenType.True or JsonTokenType.False or JsonTokenType.String or JsonTokenType.Number) {
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

    private Result<Token> ReadPrimitiveElement() {
        // Peek rune
        if (Peek() is not Rune Rune) {
            return new Error("Expected token, got end of input");
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
        else if (Options.QuotelessStrings) {
            return ReadUnquotedString();
        }
        // Invalid rune
        else {
            return new Error("Invalid rune");
        }
    }
    private Result<Token> ReadNull() {
        // Null
        return ReadLiteralToken(JsonTokenType.Null, "null", out _);
    }
    private Result<Token> ReadBoolean() {
        // Peek rune
        if (Peek() is not Rune Rune) {
            return new Error("Expected boolean, got end of input");
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
    private Result<Token> ReadString() {
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
                        if (!Options.MultiQuotedStrings) {
                            return new Error("Triple-quoted strings are not allowed");
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
                        return new Error("Single-quoted strings are not allowed");
                    }
                    OpeningQuote = new Rune('\'');
                }
            }
            // Unquoted string
            else {
                if (!Options.QuotelessStrings) {
                    return new Error("Unquoted strings are not allowed");
                }
                return ReadUnquotedString();
            }
        }

        // Create string builder
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        while (true) {
            // Read rune
            if (Read() is not Rune Rune) {
                // End of incomplete string
                if (Options.IncompleteInputs) {
                    break;
                }
                // Missing end quote
                return new Error("Expected quote to end string, got end of input");
            }

            // Closing quote
            if (Rune == OpeningQuote) {
                break;
            }
            // Escape
            else if (Rune.Value is '\\') {
                // Read escaped rune
                if (Read() is not Rune EscapedRune) {
                    return new Error("Expected escape character after `\\`, got end of input");
                }

                // Double quote
                if (EscapedRune.Value is '"') {
                    StringBuilder.Append('"');
                }
                // Single quote
                else if (EscapedRune.Value is '\'') {
                    if (!Options.SingleQuotedStrings && !Options.InvalidStringEscapeSequences) {
                        return new Error("Escaped single quotes are not allowed");
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
                    if (!ReadHexSequence(4).TryGetValue(out uint Result, out Error Error)) {
                        return Error;
                    }
                    StringBuilder.Append((char)Result);
                }
                // Unicode short hex sequence
                else if (EscapedRune.Value is 'x') {
                    if (!Options.EscapedStringShortHexSequences && !Options.InvalidStringEscapeSequences) {
                        return new Error("Escaped short hex sequences are not allowed");
                    }
                    if (!ReadHexSequence(2).TryGetValue(out uint Result, out Error Error)) {
                        return Error;
                    }
                    StringBuilder.Append((char)Result);
                }
                // Newline
                else if (NewlineRunes.Contains(EscapedRune)) {
                    if (!Options.EscapedStringNewlines && !Options.InvalidStringEscapeSequences) {
                        return new Error("Escaped newlines are not allowed");
                    }
                    // Join CR LF
                    if (EscapedRune.Value is '\r') {
                        TryRead('\n');
                    }
                }
                // Invalid escape character
                else {
                    if (!Options.InvalidStringEscapeSequences) {
                        return new Error("Invalid escape character after `\\`");
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
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        while (true) {
            // Read rune
            if (Read() is not Rune Rune) {
                break;
            }

            // Newline
            if (NewlineRunes.Contains(Rune)) {
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
    private Result<Token> ReadTripleQuotedString(Rune OpeningQuote, int OpeningQuoteCount = 3) {
        long TokenPosition = Position;

        // Create string builder
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        int ClosingQuoteCounter = 0;

        while (true) {
            // Read rune
            if (Read() is not Rune Rune) {
                // End of incomplete triple-quoted string
                if (Options.IncompleteInputs) {
                    break;
                }
                // Missing end quotes
                return new Error("Expected quotes to end triple-quoted string, got end of input");
            }

            // Closing quote
            if (Rune == OpeningQuote) {
                ClosingQuoteCounter++;
                if (ClosingQuoteCounter == OpeningQuoteCount) {
                    break;
                }
            }
            // Newline
            else if (NewlineRunes.Contains(Rune)) {
                // Join CR LF
                if (Rune.Value is '\r') {
                    TryRead('\n');
                }

                StringBuilder.Append(Rune);
            }
            // Whitespace
            else if (Rune.IsWhiteSpace(Rune)) {
                StringBuilder.Append(Rune);
            }
            // Rune
            else {
                // Reset closing triple-quote counter
                ClosingQuoteCounter = 0;

                StringBuilder.Append(Rune);
            }
        }

        // Trim leading whitespace in multiline string
        if (OpeningQuoteCount > 1) {
            // Count leading whitespace preceding closing quotes
            int LastNewlineIndex = StringBuilder.AsSpan().LastIndexOfAny(NewlineChars);
            if (LastNewlineIndex != -1) {
                int LeadingWhitespaceCount = StringBuilder.Length - LastNewlineIndex;

                // Remove leading whitespace from each line
                if (LeadingWhitespaceCount > 0) {
                    int CurrentLeadingWhitespace = 0;
                    bool IsLeadingWhitespace = true;

                    for (int Index = 0; Index < StringBuilder.Length; Index++) {
                        char Char = StringBuilder[Index];

                        // Newline
                        if (NewlineChars.Contains(Char)) {
                            // Reset leading whitespace counter
                            CurrentLeadingWhitespace = 0;
                            // Enter leading whitespace
                            IsLeadingWhitespace = true;
                        }
                        // Leading whitespace
                        else if (IsLeadingWhitespace && CurrentLeadingWhitespace <= LeadingWhitespaceCount) {
                            // Whitespace
                            if (char.IsWhiteSpace(Char)) {
                                // Increment leading whitespace counter
                                CurrentLeadingWhitespace++;
                                // Maximum leading whitespace reached
                                if (CurrentLeadingWhitespace == LeadingWhitespaceCount) {
                                    // Remove leading whitespace
                                    StringBuilder.Remove(Index - CurrentLeadingWhitespace, CurrentLeadingWhitespace);
                                    // Exit leading whitespace
                                    IsLeadingWhitespace = false;
                                }
                            }
                            // Non-whitespace
                            else {
                                // Remove partial leading whitespace
                                StringBuilder.Remove(Index - CurrentLeadingWhitespace, CurrentLeadingWhitespace);
                                // Exit leading whitespace
                                IsLeadingWhitespace = false;
                            }
                        }
                    }

                    // Remove leading whitespace from last line
                    StringBuilder.Remove(StringBuilder.Length - LeadingWhitespaceCount, LeadingWhitespaceCount);

                    // Remove leading newline
                    foreach (char NewlineChar in NewlineChars) {
                        // Found leading newline
                        if (StringBuilder.AsSpan().StartsWith([NewlineChar])) {
                            int NewlineLength = 1;
                            // Join CR LF
                            if (NewlineChar is '\r' && StringBuilder.AsSpan().StartsWith("\r\n")) {
                                NewlineLength = 2;
                            }

                            // Remove leading newline
                            StringBuilder.Remove(0, NewlineLength);
                            break;
                        }
                    }
                }
            }
        }

        // End token
        return new Token(this, JsonTokenType.String, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private Result<Token> ReadNumber() {
        long TokenPosition = Position;

        Result<Token> ReadNumberNoFallback() {
            // Create string builder
            using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

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
                    return new Error("Explicit plus-signs are not allowed");
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
                    if (!ReadLiteralToken(JsonTokenType.String, Literal, out bool UnquotedStringFallback).TryGetValue(out Token LiteralToken, out Error Error)) {
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
                    return new Error("Leading decimal points are not allowed");
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
                        return new Error("Duplicate exponent");
                    }
                    if (TrailingSign) {
                        return new Error("Expected digit before exponent");
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
                        return new Error("Duplicate decimal point");
                    }
                    if (ParsedExponent) {
                        return new Error("Exponent cannot be fractional");
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
                        return new Error("Hexadecimal specifier must be at the start of the number");
                    }
                    if (!Options.HexadecimalNumbers) {
                        return new Error("Hexadecimal numbers are not allowed");
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
                            return new Error("Expected digit after decimal point");
                        }
                    }
                    if (TrailingExponent) {
                        return new Error("Expected digit after exponent");
                    }
                    if (TrailingSign) {
                        return new Error("Expected digit after sign");
                    }
                    if (!Options.LeadingZeroes) {
                        if (LeadingZero && ParsedNonZeroDigit) {
                            return new Error("Leading zeroes are not allowed");
                        }
                    }

                    // Detect unquoted string (e.g. `123 a`)
                    if (Options.QuotelessStrings) {
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

        Result<Token> NumberToken = ReadNumberNoFallback();

        // Fallback to unquoted string
        if (NumberToken.IsError && Options.QuotelessStrings) {
            Position = TokenPosition;
            return ReadUnquotedString();
        }

        return NumberToken;
    }
    private IEnumerable<Result<Token>> ReadObject(bool OmitBraces, bool IsRoot = true) {
        // Opening brace
        if (!OmitBraces) {
            if (!TryRead('{')) {
                yield return new Error("Expected `{` to start object");
                yield break;
            }
            yield return new Token(this, JsonTokenType.StartObject, Position - 1);
        }
        // Start of object with omitted braces
        else {
            yield return new Token(this, JsonTokenType.StartObject, Position);
        }

        // Comments & whitespace
        foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
            if (Token.IsError) {
                yield return Token.Error;
                yield break;
            }
            yield return Token;
        }

        bool PropertyLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                // End of incomplete object
                if (Options.IncompleteInputs) {
                    yield return new Token(this, JsonTokenType.EndObject, Position);
                    yield break;
                }
                // End of object with omitted braces
                if (OmitBraces) {
                    yield return new Token(this, JsonTokenType.EndObject, Position);
                    yield break;
                }
                // Missing closing brace
                yield return new Error("Expected `}` to end object, got end of input");
                yield break;
            }

            // Closing brace
            if (Rune.Value is '}') {
                // End of object with omitted braces
                if (OmitBraces) {
                    yield return new Token(this, JsonTokenType.EndObject, Position);
                    yield break;
                }
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        yield return new Error("Trailing commas are not allowed");
                        yield break;
                    }
                }
                // End of object
                yield return new Token(this, JsonTokenType.EndObject, Position);
                Read();
                yield break;
            }
            // Closing bracket
            else if (Rune.Value is ']') {
                // End of object with omitted braces
                if (OmitBraces) {
                    yield return new Token(this, JsonTokenType.EndObject, Position);
                    yield break;
                }
                // Unexpected closing bracket
                yield return new Error("Expected `}` to end object, got `]`");
                yield break;
            }
            // Property name
            else {
                // Unexpected property name
                if (!PropertyLegal) {
                    yield return new Error("Expected `,` before property name in object");
                    yield break;
                }

                // Property name
                foreach (Result<Token> Token in ReadPropertyName()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Property value
                foreach (Result<Token> Token in ReadElement(IsRoot: false)) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Comma
                TrailingComma = TryRead(',');
                PropertyLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }
            }
        }
    }
    private IEnumerable<Result<Token>> ReadPropertyName() {
        long TokenPosition = Position;

        // Unquoted property name
        if (Peek()?.Value is not ('"' or '\'')) {
            // ECMAScript property name
            if (Options.EcmaScriptPropertyNames) {
                foreach (Result<Token> Token in ReadEcmaScriptPropertyName()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }
                yield break;
            }
            // Unquoted property name
            else if (Options.QuotelessPropertyNames) {
                yield return ReadUnquotedPropertyName();
                yield break;
            }
            else {
                yield return new Error("Unquoted property names are not allowed");
            }
        }

        // String
        if (!ReadString().TryGetValue(out Token String, out Error StringError)) {
            yield return StringError;
            yield break;
        }

        // Comments & whitespace
        foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
            if (Token.IsError) {
                yield return Token.Error;
                yield break;
            }
            yield return Token;
        }

        // Colon
        if (!TryRead(':')) {
            yield return new Error("Expected `:` after property name in object");
            yield break;
        }
        yield return new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, String.Value);
    }
    private IEnumerable<Result<Token>> ReadEcmaScriptPropertyName() {
        long TokenPosition = Position;

        // Start token
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return [new Error("Expected `:` after property name in object")];
            }

            // Colon
            if (Rune.Value is ':') {
                Read();
                break;
            }
            // Comments & whitespace
            else if (Rune.Value is '#' or '/' || Rune.IsWhiteSpace(Rune)) {
                // A local iterator function is used here to prevent compiler errors for ValueStringBuilder, which is a ref struct
                IEnumerable<Result<Token>> ReadCommentsAndWhitespaceThenColon() {
                    // Comments & whitespace
                    foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                        if (Token.IsError) {
                            yield return Token.Error;
                            yield break;
                        }
                        yield return Token;
                    }

                    // Colon
                    if (!TryRead(':')) {
                        yield return new Error("Expected `:` after property name in object");
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
                    return [new Error("Expected escape character after `\\`, got end of input")];
                }

                // Unicode hex sequence
                if (EscapedRune.Value is 'u') {
                    if (!ReadHexSequence(4).TryGetValue(out uint Result, out Error Error)) {
                        return [Error];
                    }
                    StringBuilder.Append((char)Result);
                }
                // Invalid escape character
                else {
                    return [new Error("Invalid escape character after `\\`")];
                }
            }
            // Unicode letter
            else if (Rune.IsLetter(Rune)) {
                Read();
                StringBuilder.Append(Rune);
            }
            // Invalid rune
            else {
                return [new Error("Unexpected rune in property name")];
            }
        }

        // End token
        return [new Token(this, JsonTokenType.PropertyName, TokenPosition, Position - TokenPosition, StringBuilder.ToString())];
    }
    private Result<Token> ReadUnquotedPropertyName() {
        long TokenPosition = Position;

        // Start token
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return new Error("Expected `:` after property name in object");
            }

            // Colon
            if (Rune.Value is ':') {
                Read();
                break;
            }
            // Invalid rune
            else if (Rune.Value is ',' or ':' or '[' or ']' or '{' or '}' || Rune.IsWhiteSpace(Rune)) {
                return new Error("Unexpected rune in property name");
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
    private IEnumerable<Result<Token>> ReadArray(bool IsRoot = true) {
        // Opening bracket
        if (!TryRead('[')) {
            yield return new Error("Expected `[` to start array");
            yield break;
        }
        yield return new Token(this, JsonTokenType.StartArray, Position - 1);

        // Comments & whitespace
        foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
            if (Token.IsError) {
                yield return Token.Error;
                yield break;
            }
            yield return Token;
        }

        bool ItemLegal = true;
        bool TrailingComma = false;

        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                // End of incomplete array
                if (Options.IncompleteInputs) {
                    yield return new Token(this, JsonTokenType.EndObject, Position);
                    yield break;
                }
                // Missing closing bracket
                yield return new Error("Expected `]` to end array, got end of input");
                yield break;
            }

            // Closing bracket
            if (Rune.Value is ']') {
                // Trailing comma
                if (TrailingComma) {
                    if (!Options.TrailingCommas) {
                        yield return new Error("Trailing commas are not allowed");
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
                    yield return new Error("Expected `,` before item in array");
                    yield break;
                }

                // Item
                foreach (Result<Token> Token in ReadElement(IsRoot: false)) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Comments & whitespace
                foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }

                // Comma
                TrailingComma = TryRead(',');
                ItemLegal = TrailingComma || Options.OmittedCommas;

                // Comments & whitespace
                foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                    if (Token.IsError) {
                        yield return Token.Error;
                        yield break;
                    }
                    yield return Token;
                }
            }
        }
    }
    private IEnumerable<Result<Token>> ReadCommentsAndWhitespace() {
        while (true) {
            // Whitespace
            if (ReadWhitespace().TryGetError(out Error WhitespaceError)) {
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
    private Result<Token> ReadComment() {
        long TokenPosition = Position;

        // Comment type
        bool IsBlockComment = false;
        if (TryRead('/')) {
            // Line-style comment
            if (TryRead('/')) {
                // Ensure line-style comments are enabled
                if (!Options.LineStyleComments) {
                    return new Error("Line-style comments are not allowed");
                }
            }
            // Block-style comment
            else if (TryRead('*')) {
                // Ensure block-style comments are enabled
                if (!Options.BlockStyleComments) {
                    return new Error("Block-style comments are not allowed");
                }
                IsBlockComment = true;
            }
        }
        // Hash-style comment
        else if (TryRead('#')) {
            // Ensure hash-style comments are enabled
            if (!Options.HashStyleComments) {
                return new Error("Hash-style comments are not allowed");
            }
        }
        // Invalid comment
        else {
            return new Error("Expected `#` or `//` or `/*` to start comment");
        }

        // Create string builder
        using ValueStringBuilder StringBuilder = new(stackalloc char[32]);

        // Read comment
        while (true) {
            // Read rune
            if (Read() is not Rune CommentRune) {
                if (IsBlockComment) {
                    // End of incomplete block comment
                    if (Options.IncompleteInputs) {
                        break;
                    }
                    // Missing closing block comment
                    return new Error("Expected `*/` to end block-style comment, got end of input");
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
                if (NewlineRunes.Contains(CommentRune)) {
                    break;
                }
            }

            // Append rune to comment
            StringBuilder.Append(CommentRune.Value);
        }

        // Create comment token
        return new Token(this, JsonTokenType.Comment, TokenPosition, Position - TokenPosition, StringBuilder.ToString());
    }
    private Result ReadWhitespace() {
        while (true) {
            // Peek rune
            if (Peek() is not Rune Rune) {
                return Result.Success;
            }

            // JSON whitespace
            if (Rune.Value is '\n' or '\r' or ' ' or '\t') {
                Read();
            }
            // All whitespace
            else if (Rune.IsWhiteSpace(Rune)) {
                if (!Options.AllWhitespace) {
                    return new Error("Non-JSON whitespace is not allowed");
                }
                Read();
            }
            // End of whitespace
            else {
                return Result.Success;
            }
        }
    }
    private Result<Token> ReadLiteralToken(JsonTokenType TokenType, scoped ReadOnlySpan<char> Literal, out bool UnquotedStringFallback) {
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
                if (!Options.QuotelessStrings) {
                    return new Error("Unquoted strings are not allowed");
                }
                Position = TokenPosition;
                return ReadUnquotedString();
            }
        }
        UnquotedStringFallback = false;
        return new Token(this, TokenType, TokenPosition, Literal.Length);
    }
    private Result<uint> ReadHexSequence(int Length) {
        Span<char> HexChars = stackalloc char[Length];

        for (int Index = 0; Index < Length; Index++) {
            Rune? Rune = Read();

            // Hex digit
            if (Rune?.Value is (>= '0' and <= '9') or (>= 'A' and <= 'F') or (>= 'a' and <= 'f')) {
                HexChars[Index] = (char)Rune.Value.Value;
            }
            // Unexpected char
            else {
                return new Error("Incorrect number of hexadecimal digits in unicode escape sequence");
            }
        }

        // Parse unicode character from hex digits
        return uint.Parse(HexChars, NumberStyles.AllowHexSpecifier);
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
    private bool DetectObjectWithOmittedBraces() {
        long StartTestPosition = Position;
        try {
            // Comments & whitespace
            foreach (Result<Token> Token in ReadCommentsAndWhitespace()) {
                if (Token.IsError) {
                    return false;
                }
            }

            // Property name (including colon)
            foreach (Result<Token> Token in ReadPropertyName()) {
                if (Token.IsError) {
                    return false;
                }
            }

            // We read a property name (e.g. `a:`), so must be an object with omitted braces
            return true;
        }
        finally {
            Position = StartTestPosition;
        }
    }

    /// <summary>
    /// A single token for a <see cref="JsonTokenType"/> in a <see cref="JsonReader"/>.
    /// </summary>
    public readonly record struct Token(CustomJsonReader JsonReader, JsonTokenType JsonType, long Position, long Length = 1, string Value = "") {
        /// <summary>
        /// Parses a single element at the token's position in the <see cref="JsonReader"/>.
        /// </summary>
        public Result<T?> ParseElement<T>(bool IsRoot = true) {
            // Go to token position
            long OriginalPosition = JsonReader.Position;
            JsonReader.Position = Position;
            try {
                // Parse element
                return JsonReader.ParseElement<T>(IsRoot);
            }
            finally {
                // Return to original position
                JsonReader.Position = OriginalPosition;
            }
        }
        /// <inheritdoc cref="ParseElement{T}(bool)"/>
        public Result<JsonElement> ParseElement(bool IsRoot = true) {
            return ParseElement<JsonElement>(IsRoot);
        }
    }
}