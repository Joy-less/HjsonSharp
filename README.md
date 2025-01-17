# HjsonSharp

A customisable streaming parser for [HJSON](https://hjson.github.io), with support for [JSON](https://json.org), [JSONC](https://code.visualstudio.com/docs/languages/json#_json-with-comments) and [JSON5](https://json5.org).

## Features

- **System.Text.Json integrated:** Parse HJSON directly to `System.Text.JsonElement`.
- **Token streaming:** Parse one token at a time without loading the entire document into memory.
- **Feature switches:** Pick and choose your desired non-JSON features, with presets for HJSON, JSONC and JSON5.
- **Unicode compatible:** Compatible with UTF-8, UTF-16, UTF-32 and ASCII encodings.
- **Performant:** Frequently uses spans and value types to avoid unnecessary allocations.

## TODO

- **Incomplete documents:** Parse incomplete documents (such as `{"key": "val`).

## Specification Differences

### Leading Whitespace In Triple Quoted Strings (HJSON)

Instead of counting leading whitespace preceding the opening quotes, HjsonSharp counts leading whitespace preceding the closing quotes.

See the [open issue](https://github.com/hjson/hjson/issues/132) for more information.

For maximum portability, don't rely on significant leading whitespace in triple-quoted strings.

### Carriage Returns (HJSON)

In the HJSON specification, carriage returns (`\r`) are ignored in favour of line feeds (`\n`). However, HjsonSharp allows `\n`, `\r` and `\r\n`.

This affects unquoted strings, which are terminated by a newline, and triple-quoted strings, which trim the first and last newlines.

For maximum portability, always use the line feed (`\n`) newline style in documents.

### Escaped Newlines In Strings (HJSON)

The official [HJSON specification](https://hjson.github.io/rfc.html) does not include support for escaping newlines in strings like JSON5.

However, an [open issue](https://github.com/hjson/hjson/issues/106) to add this was approved by a maintainer:

> I no longer have any objections, implementing this for single or double quoted strings only should be fine. Not for multiline and not for quoteless strings. I would support PR:s for this, but we need PR:s for all major supported languages before changing the syntax documentation.

As such, HjsonSharp supports escaped newlines in single and double quoted strings by default.

For maximum portability, avoid escaping newlines in strings.