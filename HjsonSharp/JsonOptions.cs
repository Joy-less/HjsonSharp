﻿using System.Text.Json.Serialization;
using System.Text.Json;

namespace HjsonSharp;

public static class JsonOptions {
    public static JsonSerializerOptions Mini { get; } = new() {
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,
        AllowTrailingCommas = true,
        IncludeFields = true,
        NewLine = "\n",
        ReadCommentHandling = JsonCommentHandling.Allow,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
    public static JsonSerializerOptions Pretty { get; } = new(Mini) {
        WriteIndented = true,
    };

    static JsonOptions() {
        Mini.MakeReadOnly();
        Pretty.MakeReadOnly();
    }
}