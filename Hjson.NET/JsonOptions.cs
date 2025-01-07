using System.Text.Json.Serialization;
using System.Text.Json;

namespace Hjson.NET;

public static class JsonOptions {
    public static JsonSerializerOptions Mini { get; } = new() {
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        AllowTrailingCommas = true,
        IncludeFields = true,
        NewLine = "\n",
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    public static JsonSerializerOptions Pretty { get; } = new(Mini) {
        WriteIndented = true,
    };
}