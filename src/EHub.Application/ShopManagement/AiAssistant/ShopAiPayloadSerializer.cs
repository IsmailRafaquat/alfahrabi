using System.Collections.Generic;
using System.Text.Json;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>Shared helper every handler uses to turn the untrusted Parameters dictionary (or a stored payload JsonElement) into its specific typed *AiCommand.</summary>
internal static class ShopAiPayloadSerializer
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static T? Deserialize<T>(Dictionary<string, JsonElement> parameters)
    {
        var json = JsonSerializer.Serialize(parameters);
        return JsonSerializer.Deserialize<T>(json, Options);
    }

    public static T? Deserialize<T>(JsonElement payload)
    {
        return JsonSerializer.Deserialize<T>(payload.GetRawText(), Options);
    }

    public static string SerializeParameters(Dictionary<string, JsonElement> parameters) => JsonSerializer.Serialize(parameters);
}
