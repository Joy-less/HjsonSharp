# HjsonSharp

A customisable streaming parser for [HJSON](https://hjson.github.io), with support for [JSON](https://json.org), [JSONC](https://code.visualstudio.com/docs/languages/json#_json-with-comments) and [JSON5](https://json5.org).

## Features

- **System.Text.Json integrated:** Parse HJSON directly to `System.Text.JsonElement`.
- **Token streaming:** Parse one token at a time without loading the entire document into memory.
- **Feature switches:** Pick and choose your desired non-JSON features, with presets for HJSON, JSONC and JSON5.
- **Unicode compatible:** Compatible with UTF-8, UTF-16, UTF-32 and ASCII encodings.
- **Performant:** Frequently uses spans and value types to avoid unnecessary allocations.

## Todo

- **Incomplete documents:** Parse incomplete documents (such as `{"key": "val`).

## Specification Differences

HjsonSharp has some minor differences to the specifications of some formats.

Here are the known differences:

### Non-UTF8 Encodings (affects HJSON)

To make the parser more consistent, HJSONSharp allows non-UTF8-encoded HJSON (unless the UTF-8 encoding is explicitly passed).

### Leading Whitespace In Triple Quoted Strings (affects HJSON)

Instead of counting leading whitespace preceding the opening quotes, HjsonSharp counts leading whitespace preceding the closing quotes.
See the [proposal](https://github.com/hjson/hjson/issues/132) for more information.