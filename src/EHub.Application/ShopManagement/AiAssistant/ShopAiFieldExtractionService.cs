using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiFieldExtractionService : IShopAiFieldExtractionService, ITransientDependency
{
    private readonly IShopOllamaClient _ollamaClient;

    public ShopAiFieldExtractionService(IShopOllamaClient ollamaClient)
    {
        _ollamaClient = ollamaClient;
    }

    public async Task<Dictionary<string, JsonElement>> ExtractAsync(
        ShopAiModuleMetadata module, IReadOnlyList<ShopAiFieldMetadata> targetFields, string userMessage, CancellationToken cancellationToken = default)
    {
        var systemPrompt = BuildExtractionPrompt(module, targetFields);
        var chatResult = await _ollamaClient.ChatAsync(systemPrompt, userMessage, cancellationToken);
        if (!chatResult.Success || string.IsNullOrWhiteSpace(chatResult.Content))
        {
            return new Dictionary<string, JsonElement>();
        }

        var jsonText = ExtractJsonObject(chatResult.Content);
        if (jsonText == null) return new Dictionary<string, JsonElement>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new Dictionary<string, JsonElement>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, JsonElement>();
        }
    }

    private static string BuildExtractionPrompt(ShopAiModuleMetadata module, IReadOnlyList<ShopAiFieldMetadata> targetFields)
    {
        var fieldDescriptions = string.Join("\n", targetFields.Select(f =>
        {
            var type = f.AllowedValues.Count > 0 ? $"one of [{string.Join(", ", f.AllowedValues)}]" : f.DataType;
            return $"- \"{f.FieldKey}\": {type} - {f.DisplayName}: {f.Description}";
        }));

        return $$"""
You are extracting field values for a "{{module.DisplayName}}" record in a Shop Management system, from a user message in English, Urdu, or Roman Urdu. The message may also contain an instruction to create/add/save the record - ignore that instruction, extract only the field values.

Extract ONLY these fields, using these exact JSON keys:
{{fieldDescriptions}}

Rules:
- Only include a field if the user's message actually states a value for it.
- Never invent a value. If the message does not mention a field, omit that key entirely.
- If more than one field above is Boolean, match each stated value to the SPECIFIC field it describes by its meaning - do not assume a boolean word applies to the first/only boolean field listed.
- Boolean fields must be a real JSON true/false, never the word itself.
  - true: true, yes, y, haan, han, ha, ji, active, enable, enabled, allow, allowed.
  - false: false, no, n, nahi, nahin, inactive, disable, disabled, "not allowed", "allow nahi", "decimal nahi".
- Numeric fields must be a real JSON number, not a string.
- Do not include any field not listed above.

Respond with ONLY a single JSON object of the extracted fields - no markdown, no explanation, no reasoning trace. If nothing can be extracted, respond with {}.
""";
    }

    private static string? ExtractJsonObject(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        return start < 0 || end < start ? null : content.Substring(start, end - start + 1);
    }
}
