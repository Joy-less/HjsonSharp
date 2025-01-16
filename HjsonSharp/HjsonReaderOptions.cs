namespace HjsonSharp;

/// <summary>
/// Options used by a <see cref="HjsonReader"/> including feature switches.
/// </summary>
public record struct HjsonReaderOptions() {
    /// <summary>
    /// The standard, strict JSON format.
    /// See <see href="https://json.org"/>.
    /// </summary>
    public static HjsonReaderOptions Json => new();
    /// <summary>
    /// A variant of JSON allowing line-style comments, block-style comments, and trailing commas.<br/>
    /// <see href="https://code.visualstudio.com/docs/languages/json#_json-with-comments"/>
    /// </summary>
    public static HjsonReaderOptions Jsonc => Json with {
        LineStyleComments = true,
        BlockStyleComments = true,
        TrailingCommas = true,
    };
    /// <summary>
    /// A variant of JSON allowing ECMAScript property names, trailing commas, single-quoted strings, escaped newlines in strings, invalid escape sequences
    /// in strings, hexadecimal numbers, leading decimal points, trailing decimal points, named floating-point literals, explicit plus-signs,
    /// line-style comments, block-style comments, and unicode whitespace.<br/>
    /// <see href="https://json5.org"/>
    /// </summary>
    public static HjsonReaderOptions Json5 => Json with {
        EcmaScriptPropertyNames = true,
        TrailingCommas = true,
        SingleQuotedStrings = true,
        StringEscapedNewlines = true,
        StringInvalidEscapeSequences = true,
        HexadecimalNumbers = true,
        LeadingDecimalPoints = true,
        TrailingDecimalPoints = true,
        NamedFloatingPointLiterals = true,
        ExplicitPlusSigns = true,
        LineStyleComments = true,
        BlockStyleComments = true,
        AllWhitespace = true,
    };
    /// <summary>
    /// A variant of JSON allowing unquoted property names, trailing commas, omitted commas, single-quoted strings, triple-quoted multi-line strings,
    /// unquoted strings, line-style comments, block-style comments, hash-style comments, and omitted root object braces.<br/>
    /// <see href="https://hjson.github.io"/>
    /// </summary>
    public static HjsonReaderOptions Hjson => Json with {
        UnquotedPropertyNames = true,
        TrailingCommas = true,
        OmittedCommas = true,
        SingleQuotedStrings = true,
        TripleQuotedStrings = true,
        UnquotedStrings = true,
        LineStyleComments = true,
        BlockStyleComments = true,
        HashStyleComments = true,
        OmittedRootObjectBraces = true,
    };

    /// <summary>
    /// Enables/disables line-style comments.
    /// <code>
    /// // comment
    /// </code>
    /// </summary>
    public bool LineStyleComments { get; set; }
    /// <summary>
    /// Enables/disables block-style comments.
    /// <code>
    /// /* comment */
    /// </code>
    /// </summary>
    public bool BlockStyleComments { get; set; }
    /// <summary>
    /// Enables/disables hash-style comments.
    /// <code>
    /// # comment
    /// </code>
    /// </summary>
    public bool HashStyleComments { get; set; }
    /// <summary>
    /// Enables/disables a single trailing comma in arrays and objects.
    /// <code>
    /// [1, 2, 3,]
    /// </code>
    /// </summary>
    public bool TrailingCommas { get; set; }
    /// <summary>
    /// Enables/disables omitted commas in arrays and objects.
    /// <code>
    /// [
    ///   1
    ///   2,
    ///   3
    /// ]
    /// </code>
    /// </summary>
    public bool OmittedCommas { get; set; }
    /// <summary>
    /// Enables/disables non-JSON whitespace characters.
    /// <code>
    /// \v (vertical tab)
    /// \f (form feed)
    /// \u200A (hair space)
    /// </code>
    /// </summary>
    public bool AllWhitespace { get; set; }
    /// <summary>
    /// Enables/disables unquoted property names.
    /// <code>
    /// { a: "b" }
    /// </code>
    /// </summary>
    public bool UnquotedPropertyNames { get; set; }
    /// <summary>
    /// Enables/disables ECMAScript-style property names.
    /// <code>
    /// {
    ///   a$_b\u0065私: "b",
    /// }
    /// </code>
    /// 
    /// </summary>
    public bool EcmaScriptPropertyNames { get; set; }
    /// <summary>
    /// Enables/disables single-quoted strings.
    /// <code>
    /// 'string'
    /// </code>
    /// <see href="https://262.ecma-international.org/5.1/#sec-7.6"/>
    /// </summary>
    /// <remarks>
    /// Also enables the <c>\'</c> escape sequence in single and double quoted strings.
    /// </remarks>
    public bool SingleQuotedStrings { get; set; }
    /// <summary>
    /// Enables/disables triple-quoted multi-line strings.
    /// <code>
    /// '''
    /// string
    /// '''
    /// </code>
    /// </summary>
    /// <remarks>
    /// Note: The HJSON specification trims leading whitespace based on the whitespace preceding the opening
    /// triple quotes. However, since that would require reading backwards, this implementation uses the
    /// closing triple quotes instead (like C#). This is unlikely to make a difference.
    /// </remarks>
    public bool TripleQuotedStrings { get; set; }
    /// <summary>
    /// Enables/disables unquoted strings.
    /// <code>
    /// string
    /// </code>
    /// </summary>
    /// <remarks>
    /// Since unquoted strings are terminated by a newline, <see cref="OmittedCommas"/> should also be <see langword="true"/>.
    /// </remarks>
    public bool UnquotedStrings { get; set; }
    /// <summary>
    /// Enables/disables escaped newlines in strings.
    /// <code>
    /// "hello \
    /// world"
    /// </code>
    /// </summary>
    public bool StringEscapedNewlines { get; set; }
    /// <summary>
    /// Enables/disables the <c>\x</c> escape sequence as an alternative to <c>\u</c> in strings.
    /// <code>
    /// "\x00E7" (ç)
    /// "\x0E7" (ç)
    /// "\xE7" (ç)
    /// </code>
    /// </summary>
    public bool StringEscapedVariableLengthHexSequences { get; set; }
    /// <summary>
    /// Enables/disables non-existent escape sequences in strings.
    /// <code>
    /// "\A" (A)
    /// "\D" (D)
    /// </code>
    /// </summary>
    public bool StringInvalidEscapeSequences { get; set; }
    /// <summary>
    /// Enables/disables numbers with leading 0's.
    /// <code>
    /// 012
    /// </code>
    /// </summary>
    public bool LeadingZeroes { get; set; }
    /// <summary>
    /// Enables/disables numbers starting with a decimal point.
    /// <code>
    /// .5
    /// </code>
    /// </summary>
    public bool LeadingDecimalPoints { get; set; }
    /// <summary>
    /// Enables/disables numbers ending with a decimal point.
    /// <code>
    /// 5.
    /// </code>
    /// </summary>
    public bool TrailingDecimalPoints { get; set; }
    /// <summary>
    /// Enables/disables numbers starting with an explicit plus-sign.
    /// <code>
    /// +5
    /// </code>
    /// </summary>
    public bool ExplicitPlusSigns { get; set; }
    /// <summary>
    /// Enables/disables named literals for numbers.
    /// <code>
    /// Infinity
    /// -Infinity
    /// NaN
    /// -NaN
    /// </code>
    /// </summary>
    public bool NamedFloatingPointLiterals { get; set; }
    /// <summary>
    /// Enables/disables hexadecimal for numbers.
    /// <code>
    /// 0xDEADCAFE
    /// </code>
    /// </summary>
    public bool HexadecimalNumbers { get; set; }
    /// <summary>
    /// Enables/disables omitted braces for root objects.
    /// <code>
    /// a: 5,
    /// b: "..."
    /// </code>
    /// </summary>
    public bool OmittedRootObjectBraces { get; set; }
}