namespace HjsonSharp;

public record struct HjsonStreamOptions {
    public int BufferSize { get; set; }
    /// <summary>
    /// Enables/disables triple-quoted multi-line string literals:<br/>
    /// <c>'''string'''</c>.
    /// </summary>
    public bool RawStringLiterals { get; set; }
    /// <summary>
    /// Enables/disables hash-style comments:<br/>
    /// <c># comment</c>.
    /// </summary>
    public bool HashStyleComments { get; set; }
    /// <summary>
    /// Enables/disables line-style comments:<br/>
    /// <c>// comment</c>.
    /// </summary>
    public bool LineStyleComments { get; set; }
    /// <summary>
    /// Enables/disables block-style comments:<br/>
    /// <c>/* comment */</c>.
    /// </summary>
    public bool BlockStyleComments { get; set; }
    /// <summary>
    /// Enables/disables a single trailing comma in arrays and objects:<br/>
    /// <c>[1, 2, 3,]</c>.
    /// </summary>
    public bool TrailingCommas { get; set; }
    /// <summary>
    /// Enables/disables leading 0's in numbers:<br/>
    /// <c>012</c>.
    /// </summary>
    public bool LeadingZeroes { get; set; }
    /// <summary>
    /// Enables/disables all unicode whitespace characters:<br/>
    /// <c>\v</c>, <c>\f</c>, <c>\u200A</c>.
    /// </summary>
    public bool AllWhitespace { get; set; }

    /// <summary>
    /// The standard, strict JSON format.
    /// See <see href="https://json.org"/>.
    /// </summary>
    public static HjsonStreamOptions Json => new() {
        BufferSize = 4096,
    };
    /// <summary>
    /// A variant of JSON allowing line-style comments, block-style comments, and trailing commas.
    /// See <see href="https://code.visualstudio.com/docs/languages/json#_json-with-comments"/>.
    /// </summary>
    public static HjsonStreamOptions Jsonc => Json with {
        LineStyleComments = true,
        BlockStyleComments = true,
        TrailingCommas = true,
    };
    /// <summary>
    /// A variant of JSON allowing unquoted property names, trailing commas, single-quoted strings, escaped multi-line strings, hexadecimal numbers,
    /// leading decimal points, trailing decimal points, named floating-point literals, explicit plus-signs, line-style comments, block-style comments,
    /// and all whitespace characters.
    /// See <see href="https://json5.org"/>.
    /// </summary>
    public static HjsonStreamOptions Json5 => Json with {

    };
    public static HjsonStreamOptions Hjson => Json with {

    };
}