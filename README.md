# HjsonSharp

[![NuGet](https://img.shields.io/nuget/v/HjsonSharp.svg)](https://www.nuget.org/packages/HjsonSharp)

A customisable streaming parser for [HJSON](https://hjson.github.io), with support for [JSON](https://json.org), [JSONC](https://code.visualstudio.com/docs/languages/json#_json-with-comments) and [JSON5](https://json5.org).

## Features

- **System.Text.Json integrated:** Parse directly to `System.Text.JsonElement`.
- **Token streaming:** Parse one token at a time without loading the entire document into memory.
- **Feature switches:** Pick and choose your desired non-JSON features, with presets for HJSON, JSONC and JSON5.
- **Unicode compatible:** Compatible with UTF-8, UTF-16, UTF-32 and ASCII encodings.
- **Incomplete inputs:** Parse incomplete values, such as `{"key": "val`.
- **Result pattern:** Uses `HjsonResult` to avoid the overhead of exceptions.

## Example

```cs
string Text = """
    {
        // use #, // or /**/ comments,
        // omit quotes for keys
        key: 1
        // omit quotes for strings
        contains: everything on this line
        // omit commas at the end of a line
        cool: {
          foo: 1
          bar: 2
        }
        // allow trailing commas
        list: [
          1,
          2,
        ]
        // and use multiline strings
        realist:
          '''
          My half empty glass,
          I will fill your empty half.
          Now you are half full.
          '''
    }
    """;

// Parse to JsonElement
JsonElement Element = JsonReader.ParseElement(Text, JsonReaderOptions.Hjson).Value;

// Serialize to JSON
string Json = JsonSerializer.Serialize(Element);
```

## Specification Differences

### Escaped Newlines In Strings (HJSON)

The official [HJSON specification](https://hjson.github.io/rfc.html) does not include support for escaping newlines in strings like JSON5.

However, an [open issue](https://github.com/hjson/hjson/issues/106) to add this was approved by a maintainer:

> I no longer have any objections, implementing this for single or double quoted strings only should be fine. Not for multiline and not for quoteless strings. I would support PR:s for this, but we need PR:s for all major supported languages before changing the syntax documentation.

As such, HjsonSharp supports escaped newlines in single and double quoted strings by default.

For maximum portability, avoid escaping newlines in strings.

### Leading Whitespace In Triple Quoted Strings (HJSON)

Instead of counting leading whitespace preceding the opening quotes, HjsonSharp counts leading whitespace preceding the closing quotes.

See the [open issue](https://github.com/hjson/hjson/issues/132) for more information.

For maximum portability, don't rely on significant leading whitespace in triple-quoted strings.

### Carriage Returns And Non-Standard Newlines (HJSON)

In the HJSON specification, carriage returns (`\r`) are ignored in favour of line feeds (`\n`).
However, HjsonSharp allows all newlines supported by JSON5 (`\n`, `\r`, `\r\n`, `\u2028`, `\u2029`).

This affects unquoted strings, which are terminated by a newline, and triple-quoted strings, which trim the first and last newlines.

For maximum portability, always use the line feed (`\n`) newline style in documents.

## Benchmarks

For basic purposes, HjsonSharp has similar performance to [hjson-cs](https://github.com/hjson/hjson-cs):

| Method                 | Mean            | Error        | StdDev       | Gen0      | Gen1      | Gen2     | Allocated  |
|----------------------- |----------------:|-------------:|-------------:|----------:|----------:|---------:|-----------:|
| LongStringHjsonCs      | 11,135,676.0 ns | 25,272.15 ns | 21,103.39 ns | 1093.7500 | 1031.2500 | 734.3750 | 7828.64 KB |
| LongStringHjsonSharp   | 19,753,082.7 ns | 71,314.79 ns | 66,707.90 ns |  375.0000 |  375.0000 | 375.0000 | 9956.86 KB |
| ShortIntegerHjsonCs    |      3,952.6 ns |     18.41 ns |     15.37 ns |    0.4578 |         - |        - |    1.41 KB |
| ShortIntegerHjsonSharp |        615.7 ns |      3.08 ns |      2.88 ns |    0.3519 |         - |        - |    1.08 KB |
| PersonHjsonCs          |      2,440.2 ns |     18.83 ns |     17.61 ns |    1.0376 |         - |        - |    3.19 KB |
| PersonHjsonSharp       |      4,658.1 ns |     22.55 ns |     21.09 ns |    2.5177 |         - |        - |    7.73 KB |