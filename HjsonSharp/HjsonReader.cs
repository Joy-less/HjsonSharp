﻿using LinkDotNet.StringBuilder;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

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
    /// Parses a single element from the stream.
    /// </summary>
    public static T? ParseElement<T>(Stream Stream, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(Stream, Encoding, Options);
        return HjsonReader.ParseElement<T>();
    }
    /// <inheritdoc cref="ParseElement{T}(Stream, Encoding?, HjsonReaderOptions?)"/>
    public static JsonElement ParseElement(Stream Stream, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(Stream, Encoding, Options);
    }
    /// <summary>
    /// Parses a single element from the byte array.
    /// </summary>
    public static T? ParseElement<T>(byte[] Bytes, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(Bytes, Encoding, Options);
        return HjsonReader.ParseElement<T>();
    }
    /// <inheritdoc cref="ParseElement{T}(byte[], Encoding?, HjsonReaderOptions?)"/>
    public static JsonElement ParseElement(byte[] Bytes, Encoding? Encoding = null, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(Bytes, Encoding, Options);
    }
    /// <summary>
    /// Parses a single element from the string.
    /// </summary>
    public static T? ParseElement<T>(string String, HjsonReaderOptions? Options = null) {
        using HjsonReader HjsonReader = new(String, Options);
        return HjsonReader.ParseElement<T>();
    }
    /// <inheritdoc cref="ParseElement{T}(string, HjsonReaderOptions?)"/>
    public static JsonElement ParseElement(string String, HjsonReaderOptions? Options = null) {
        return ParseElement<JsonElement>(String, Options);
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
    public override Rune? PeekRune() {
        return InnerRuneReader.PeekRune();
    }
    /// <inheritdoc/>
    public override Rune? ReadRune() {
        return InnerRuneReader.ReadRune();
    }
    /// <inheritdoc/>
    public override bool ReadRune(Rune? Expected) {
        return InnerRuneReader.ReadRune(Expected);
    }
    /// <inheritdoc/>
    public override bool ReadRune(char Expected) {
        return InnerRuneReader.ReadRune(Expected);
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
    public T? ParseElement<T>() {
        return ParseNode().Deserialize<T>(JsonOptions.Mini);
    }
    /// <inheritdoc cref="ParseElement{T}()"/>
    public JsonElement ParseElement() {
        return ParseElement<JsonElement>();
    }
    /// <summary>
    /// Tries to parse a single element from the stream, returning <see langword="false"/> if an exception occurs.
    /// </summary>
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
    /// <inheritdoc cref="TryParseElement{T}(out T)"/>
    public bool TryParseElement(out JsonElement Result) {
        return TryParseElement<JsonElement>(out Result);
    }
    /// <summary>
    /// Parses a single <see cref="JsonNode"/> from the stream.
    /// </summary>
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
                JsonNode Node = CreateJsonValueFromNumberString(Token.Value);
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
    /// <summary>
    /// Tries to parse a single <see cref="JsonNode"/> from the stream, returning <see langword="false"/> if an exception occurs.
    /// </summary>
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
    /// <summary>
    /// Reads the tokens of a single element from the stream.
    /// </summary>
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
        bool ParsedNonZeroDigit = false;
        bool TrailingExponent = false;
        bool TrailingDecimalPoint = false;
        bool TrailingSign = false;
        bool LeadingZero = false;

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
                if (!Options.TrailingDecimalPoints) {
                    if (TrailingDecimalPoint) {
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
                if (!Options.LeadingZeroes) {
                    if (LeadingZero && ParsedNonZeroDigit) {
                        if (Options.UnquotedStrings) {
                            Position = TokenPosition;
                            return ReadUnquotedString();
                        }
                        throw new HjsonException("Leading zeroes are not allowed");
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
    private Token ReadLiteralToken(JsonTokenType TokenType, scoped ReadOnlySpan<char> Literal, out bool UnquotedStringFallback) {
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
    private static JsonValue CreateJsonValueFromNumberString(string NumberString) {
        // Transform leading/trailing decimal points
        if (NumberString.StartsWith('.')) {
            NumberString = '0' + NumberString;
        }
        if (NumberString.EndsWith('.')) {
            NumberString += '0';
        }
        // Parse number
        return JsonSerializer.Deserialize<JsonValue>(NumberString)!;
    }

    /// <summary>
    /// A single token for a <see cref="JsonTokenType"/> in a <see cref="HjsonReader"/>.
    /// </summary>
    public readonly record struct Token(HjsonReader HjsonReader, JsonTokenType Type, long Position, long Length = 1, string Value = "") {
        /// <summary>
        /// Parses a single element at the token's position in the <see cref="HjsonReader"/>.
        /// </summary>
        public T? ParseElement<T>() {
            // Go to token position
            long OriginalPosition = HjsonReader.Position;
            HjsonReader.Position = Position;
            try {
                // Parse element
                return HjsonReader.ParseElement<T>();
            }
            finally {
                // Return to original position
                HjsonReader.Position = OriginalPosition;
            }
        }
        /// <inheritdoc cref="ParseElement{T}()"/>
        public JsonElement ParseElement() {
            return ParseElement<JsonElement>();
        }
    }
}