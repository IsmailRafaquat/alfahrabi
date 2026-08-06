using System;
using System.Collections.Generic;
using System.Text.Json;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Ollama is instructed to return real JSON true/false for boolean fields, but a small local model
/// sometimes returns the word instead (e.g. "haan", "No") - this is the single place that word list
/// is defined, so a boolean-typed field's value is always coerced the same way regardless of which
/// caller (command parser output, slot-filling extraction, a handler's own payload) touches it.
/// </summary>
public static class ShopAiBooleanParser
{
    private static readonly HashSet<string> TrueWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "true", "yes", "y", "haan", "han", "ha", "ji", "active", "enable", "enabled", "allow", "allowed",
    };

    private static readonly HashSet<string> FalseWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "false", "no", "n", "nahi", "nahin", "inactive", "disable", "disabled", "not allowed", "decimal allow nahi",
    };

    public static bool? TryParse(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String => TryParseWord(value.GetString()),
        JsonValueKind.Number when value.TryGetInt32(out var n) => n != 0,
        _ => null,
    };

    public static bool? TryParseWord(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var trimmed = text.Trim();

        if (TrueWords.Contains(trimmed)) return true;
        if (FalseWords.Contains(trimmed)) return false;

        // Phrase-level match for short sentences like "decimal nahi allow karna" that won't match
        // a single-word lookup - check false phrases first since "not allowed" contains "allowed".
        foreach (var word in FalseWords)
        {
            if (trimmed.Contains(word, StringComparison.OrdinalIgnoreCase)) return false;
        }
        foreach (var word in TrueWords)
        {
            if (trimmed.Contains(word, StringComparison.OrdinalIgnoreCase)) return true;
        }

        return null;
    }

    /// <summary>Re-encodes a coerced bool back into a JsonElement, for storing in a Parameters/CollectedValues dictionary that only holds JsonElement values.</summary>
    public static JsonElement ToJsonElement(bool value) =>
        JsonSerializer.SerializeToElement(value);
}
