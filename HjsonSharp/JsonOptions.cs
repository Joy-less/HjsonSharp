using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;

namespace HjsonSharp;

public static class JsonOptions {
    public static JsonSerializerOptions Mini { get; } = new() {
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        AllowTrailingCommas = true,
        IncludeFields = true,
        NewLine = "\n",
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    public static JsonSerializerOptions Pretty { get; } = new(Mini) {
        WriteIndented = true,
    };
}