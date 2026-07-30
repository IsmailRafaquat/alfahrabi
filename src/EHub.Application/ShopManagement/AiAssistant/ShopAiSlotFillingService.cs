using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiSlotFillingService : IShopAiSlotFillingService, ITransientDependency
{
    // "Suggested information-collection expiry: 30 minutes" - a fixed, generous default rather
    // than a config knob, since it is a UX/abandonment concern, not an infrastructure one.
    private static readonly TimeSpan CollectingExpiry = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ReadyForConfirmationHoldover = TimeSpan.FromMinutes(5);

    private readonly IRepository<ShopAiPendingAction, Guid> _pendingActionRepository;
    private readonly ShopAiPendingActionManager _pendingActionManager;
    private readonly IShopAiModuleMetadataProvider _moduleMetadataProvider;
    private readonly IShopOllamaClient _ollamaClient;
    private readonly IClock _clock;

    public ShopAiSlotFillingService(
        IRepository<ShopAiPendingAction, Guid> pendingActionRepository,
        ShopAiPendingActionManager pendingActionManager,
        IShopAiModuleMetadataProvider moduleMetadataProvider,
        IShopOllamaClient ollamaClient,
        IClock clock)
    {
        _pendingActionRepository = pendingActionRepository;
        _pendingActionManager = pendingActionManager;
        _moduleMetadataProvider = moduleMetadataProvider;
        _ollamaClient = ollamaClient;
        _clock = clock;
    }

    public async Task<ShopAiSlotFillingResult> StartAsync(
        ShopAiConversation conversation, Guid userId, Guid sourceMessageId, ShopAiModuleMetadata module, ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        var collected = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        MergeProvidedValues(module, collected, command.Parameters);

        var result = Evaluate(module, collected);
        if (result.IsComplete)
        {
            return result;
        }

        var pendingAction = _pendingActionManager.Create(userId, conversation.Id, sourceMessageId, module.CreateAction, module.ModuleKey, _clock.Now.Add(CollectingExpiry));
        _pendingActionManager.UpdateCollectedState(pendingAction, SerializeValues(collected), JsonSerializer.Serialize(result.MissingRequiredFields), "[]", _clock.Now.Add(CollectingExpiry));
        await _pendingActionRepository.InsertAsync(pendingAction, autoSave: true);

        result.PendingAction = pendingAction;
        return result;
    }

    public async Task<ShopAiSlotFillingResult> ContinueAsync(ShopAiPendingAction pendingAction, string userMessage, CancellationToken cancellationToken = default)
    {
        var module = _moduleMetadataProvider.GetModule(pendingAction.ModuleKey);

        if (pendingAction.ExpiryDate < _clock.Now)
        {
            _pendingActionManager.MarkExpired(pendingAction);
            await _pendingActionRepository.UpdateAsync(pendingAction, autoSave: true);
            return new ShopAiSlotFillingResult
            {
                IsComplete = false,
                IsExpired = true,
                PendingAction = pendingAction,
                ModuleDisplayName = module.DisplayName,
            };
        }

        var collected = DeserializeValues(pendingAction.CollectedValuesJson);
        var missingBefore = Evaluate(module, collected).MissingRequiredFields;

        // Only required fields are ever asked about one at a time (see Evaluate() below), so
        // "skip" never legitimately applies here - this loop only extracts values for fields the
        // module actually needs before it can build a valid preview.
        var trimmed = userMessage.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed) && missingBefore.Count > 0)
        {
            var targetFields = module.Fields.Where(f => missingBefore.Contains(f.FieldKey, StringComparer.OrdinalIgnoreCase)).ToList();
            var extracted = await ExtractFieldValuesAsync(module, targetFields, trimmed, cancellationToken);
            MergeProvidedValues(module, collected, extracted);
        }

        var evaluated = Evaluate(module, collected);
        if (evaluated.IsComplete)
        {
            _pendingActionManager.MarkReadyForConfirmation(pendingAction, _clock.Now.Add(ReadyForConfirmationHoldover));
        }
        else
        {
            _pendingActionManager.UpdateCollectedState(pendingAction, SerializeValues(collected), JsonSerializer.Serialize(evaluated.MissingRequiredFields), "[]", _clock.Now.Add(CollectingExpiry));
        }
        await _pendingActionRepository.UpdateAsync(pendingAction, autoSave: true);

        evaluated.PendingAction = pendingAction;
        return evaluated;
    }

    // ------------------------------------------------------------------
    // Evaluation / merging
    // ------------------------------------------------------------------

    private static void MergeProvidedValues(ShopAiModuleMetadata module, Dictionary<string, JsonElement> collected, Dictionary<string, JsonElement> provided)
    {
        foreach (var (key, value) in provided)
        {
            var field = module.Fields.FirstOrDefault(f => string.Equals(f.FieldKey, key, StringComparison.OrdinalIgnoreCase));
            if (field == null) continue; // Ollama must never introduce a field the module doesn't have.
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined) continue;
            if (value.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(value.GetString())) continue;

            collected[field.FieldKey] = value;
        }
    }

    private static ShopAiSlotFillingResult Evaluate(ShopAiModuleMetadata module, Dictionary<string, JsonElement> collected)
    {
        var requiredFields = module.Fields.Where(f => f.IsRequired && !f.IsSystemGenerated && !f.IsCalculated).ToList();
        var missing = requiredFields.Where(f => !collected.ContainsKey(f.FieldKey)).Select(f => f.FieldKey).ToList();

        var collectedFieldDtos = module.Fields
            .Where(f => !f.IsSystemGenerated && !f.IsCalculated)
            .Select(f => new ShopAiPreviewFieldDto
            {
                LabelKey = f.DisplayName,
                Value = collected.TryGetValue(f.FieldKey, out var v) ? DisplayValue(v) : null,
                IsEmpty = !collected.ContainsKey(f.FieldKey),
            })
            .ToList();

        var result = new ShopAiSlotFillingResult
        {
            IsComplete = missing.Count == 0,
            ModuleDisplayName = module.DisplayName,
            CollectedFields = collectedFieldDtos,
            MissingRequiredFields = missing,
            RequiredFieldCount = requiredFields.Count,
            CompletedRequiredFieldCount = requiredFields.Count - missing.Count,
        };

        if (missing.Count > 0)
        {
            var nextField = requiredFields.First(f => f.FieldKey == missing[0]);
            result.NextQuestion = BuildQuestion(nextField);
        }
        else
        {
            result.CompletedCommand = new ShopAiParsedCommand
            {
                Action = module.CreateAction,
                Intent = ShopAiIntentType.StartRecordCreation,
                ModuleKey = module.ModuleKey,
                Confidence = 1m,
                RequiresConfirmation = true,
                Parameters = new Dictionary<string, JsonElement>(collected, StringComparer.OrdinalIgnoreCase),
            };
        }

        return result;
    }

    private static string BuildQuestion(ShopAiFieldMetadata field)
    {
        var example = string.IsNullOrWhiteSpace(field.ExampleValue) ? string.Empty : $" (e.g. {field.ExampleValue})";
        if (field.AllowedValues.Count > 0)
        {
            return $"{field.DisplayName}?{example} Options: {string.Join(", ", field.AllowedValues)}";
        }
        if (field.DataType == "Boolean")
        {
            return $"{field.DisplayName}? (yes/no){example}";
        }
        return $"Please provide {field.DisplayName}{example}.";
    }

    private static string DisplayValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.True => "Yes",
        JsonValueKind.False => "No",
        JsonValueKind.Number => value.ToString(),
        _ => value.ToString(),
    };

    private static string SerializeValues(Dictionary<string, JsonElement> values) => JsonSerializer.Serialize(values);

    private static Dictionary<string, JsonElement> DeserializeValues(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        }
    }

    // ------------------------------------------------------------------
    // Focused per-turn extraction - deliberately NOT the big command-parsing prompt. Scoping the
    // system prompt to only the still-missing fields keeps each continuation turn as fast as this
    // hardware allows and reduces the model's chances of inventing values for fields nobody asked
    // about yet.
    // ------------------------------------------------------------------

    private async Task<Dictionary<string, JsonElement>> ExtractFieldValuesAsync(ShopAiModuleMetadata module, List<ShopAiFieldMetadata> targetFields, string userMessage, CancellationToken cancellationToken)
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

    private static string BuildExtractionPrompt(ShopAiModuleMetadata module, List<ShopAiFieldMetadata> targetFields)
    {
        var fieldDescriptions = string.Join("\n", targetFields.Select(f =>
        {
            var type = f.AllowedValues.Count > 0 ? $"one of [{string.Join(", ", f.AllowedValues)}]" : f.DataType;
            return $"- \"{f.FieldKey}\": {type} - {f.DisplayName}: {f.Description}";
        }));

        return $$"""
You are extracting field values for a "{{module.DisplayName}}" record in a Shop Management system, from a short user reply in English, Urdu, or Roman Urdu.

Extract ONLY these fields, using these exact JSON keys:
{{fieldDescriptions}}

Rules:
- Only include a field if the user's message actually states a value for it.
- Never invent a value. If the message does not mention a field, omit that key entirely.
- Boolean fields must be a real JSON true/false (interpret yes/haan/ha as true, no/nahi as false).
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
