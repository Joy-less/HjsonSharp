# Hjson.NET
 
## Objectives

**System.Text.Json compatible:** Parse HJSON directly to `JsonElement`.

**Streaming parser:** Parse one token at a time, without loading the entire document into memory.

**Incomplete document:** Option to parse incomplete HJSON (such as `{"key": "val`).

**Multiple variants:** Support for the following JSON variants: JSON, JSONC, JSON5, HJSON.

**Minimal allocation:** Minimise heap allocations by using structs and spans where possible.

**Unicode compatible:** Compatible with UTF-8, UTF-16, and UTF-32.
