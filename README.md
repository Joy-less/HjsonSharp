# Hjson.NET

A customisable streaming parser for [HJSON](https://hjson.github.io), with support for [JSON](https://json.org), [JSONC](https://code.visualstudio.com/docs/languages/json#_json-with-comments) and [JSON5](https://json5.org).

## Features

- **Compatible with System.Text.Json:** Parse HJSON directly to `System.Text.JsonElement`.
- **Token streaming:** Parse one token at a time without loading the entire document into memory.
- **Customisable:** Pick and choose which extra features on top of JSON you want, with presets for HJSON, JSON, JSONC and JSON5.
- **Unicode compatible:** Compatible with UTF-8, UTF-16, UTF-32 and ASCII.

## Todo

- **Incomplete documents:** Parse incomplete documents (such as `{"key": "val`).