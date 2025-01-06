namespace HjsonSharp;

public record struct HjsonStreamOptions() {
    public int BufferSize { get; set; } = 4096;
    public JsonSyntaxOptions Syntax { get; set; } = JsonSyntaxOptions.Hjson;
}

public record struct JsonSyntaxOptions {
    /// <summary>
    /// Enables/disables hash-style comments.
    /// <code>
    /// # comment
    /// </code>
    /// </summary>
    public bool HashStyleComments { get; set; }
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
    /// Enables/disables a single trailing comma in arrays and objects.
    /// <code>
    /// [1, 2, 3,]
    /// </code>
    /// </summary>
    public bool TrailingCommas { get; set; }
    /// <summary>
    /// Enables/disables all unicode whitespace characters.
    /// <code>
    /// \v
    /// \f
    /// \u200A
    /// </code>
    /// </summary>
    public bool AllWhitespace { get; set; }
    /// <summary>
    /// Enables/disables unquoted property names:
    /// <code>
    /// { a: "b" }
    /// </code>
    /// </summary>
    public bool UnquotedPropertyNames { get; set; }
    /// <summary>
    /// Enables/disables single-quoted strings:
    /// <code>
    /// 'string'
    /// </code>
    /// </summary>
    public bool SingleQuotedStrings { get; set; }
    /// <summary>
    /// Enables/disables triple-quoted multi-line strings:
    /// <code>
    /// '''
    /// string
    /// '''
    /// </code>
    /// </summary>
    public bool TripleQuotedMultiLineStrings { get; set; }
    /// <summary>
    /// Enables/disables unquoted strings:
    /// <code>
    /// string
    /// </code>
    /// </summary>
    public bool UnquotedStrings { get; set; }
    /// <summary>
    /// Enables/disables escaped newlines in strings:
    /// <code>
    /// "hello \
    /// world"
    /// </code>
    /// </summary>
    public bool EscapedStringNewlines { get; set; }
    /// <summary>
    /// Enables/disables escaped single quotes in strings:
    /// <code>
    /// "\'"
    /// </code>
    /// </summary>
    public bool EscapedStringSingleQuotes { get; set; }
    /// <summary>
    /// Enables/disables numbers with leading 0's:
    /// <code>
    /// 012
    /// </code>
    /// </summary>
    public bool LeadingZeroes { get; set; }
    /// <summary>
    /// Enables/disables numbers starting with a decimal point:
    /// <code>
    /// .5
    /// </code>
    /// </summary>
    public bool LeadingDecimalPoints { get; set; }
    /// <summary>
    /// Enables/disables numbers ending with a decimal point:
    /// <code>
    /// 5.
    /// </code>
    /// </summary>
    public bool TrailingDecimalPoints { get; set; }
    /// <summary>
    /// Enables/disables numbers starting with an explicit plus-sign:
    /// <code>
    /// +5
    /// </code>
    /// </summary>
    public bool ExplicitPlusSigns { get; set; }
    /// <summary>
    /// Enables/disables named literals for numbers:
    /// <code>
    /// Infinity
    /// -Infinity
    /// NaN
    /// -NaN
    /// </code>
    /// </summary>
    public bool NamedFloatingPointLiterals { get; set; }
    /// <summary>
    /// Enables/disables hexadecimal for numbers:
    /// <code>
    /// 0xDEADCAFE
    /// </code>
    /// </summary>
    public bool HexadecimalNumbers { get; set; }
    /// <summary>
    /// Enables/disables omitted braces for root objects:
    /// <code>
    /// a: 5,
    /// b: "..."
    /// </code>
    /// </summary>
    public bool OmittedRootObjectBraces { get; set; }

    /// <summary>
    /// The standard, strict JSON format.
    /// See <see href="https://json.org"/>.
    /// </summary>
    public static JsonSyntaxOptions Json => new() {
    };
    /// <summary>
    /// A variant of JSON allowing line-style comments, block-style comments, and trailing commas.
    /// See <see href="https://code.visualstudio.com/docs/languages/json#_json-with-comments"/>.
    /// </summary>
    public static JsonSyntaxOptions Jsonc => Json with {
        LineStyleComments = true,
        BlockStyleComments = true,
        TrailingCommas = true,
    };
    /// <summary>
    /// A variant of JSON allowing unquoted property names, trailing commas, single-quoted strings, escaped string newlines, hexadecimal numbers,
    /// leading decimal points, trailing decimal points, named floating-point literals, explicit plus-signs, line-style comments, block-style comments,
    /// and all whitespace characters.
    /// See <see href="https://json5.org"/>.
    /// </summary>
    public static JsonSyntaxOptions Json5 => Json with {
        UnquotedPropertyNames = true,
        TrailingCommas = true,
        SingleQuotedStrings = true,
        EscapedStringNewlines = true,
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
    /// A variant of JSON allowing unquoted property names, trailing commas, single-quoted strings, triple-quoted multi-line strings, unquoted strings,
    /// escaped string single quotes, line-style comments, block-style comments, hash-style comments, and omitted root object braces.
    /// </summary>
    public static JsonSyntaxOptions Hjson => Json with {
        UnquotedPropertyNames = true,
        TrailingCommas = true,
        SingleQuotedStrings = true,
        TripleQuotedMultiLineStrings = true,
        UnquotedStrings = true,
        EscapedStringSingleQuotes = true,
        LineStyleComments = true,
        BlockStyleComments = true,
        HashStyleComments = true,
        OmittedRootObjectBraces = true,
    };
}