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
    private readonly IShopAiFieldExtractionService _fieldExtractionService;
    private readonly IClock _clock;

    public ShopAiSlotFillingService(
        IRepository<ShopAiPendingAction, Guid> pendingActionRepository,
        ShopAiPendingActionManager pendingActionManager,
        IShopAiModuleMetadataProvider moduleMetadataProvider,
        IShopAiFieldExtractionService fieldExtractionService,
        IClock clock)
    {
        _pendingActionRepository = pendingActionRepository;
        _pendingActionManager = pendingActionManager;
        _moduleMetadataProvider = moduleMetadataProvider;
        _fieldExtractionService = fieldExtractionService;
        _clock = clock;
    }

    public async Task<ShopAiSlotFillingResult> StartAsync(
        ShopAiConversation conversation, Guid userId, Guid sourceMessageId, ShopAiModuleMetadata module, ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        var collected = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        MergeProvidedValues(module, collected, command.Parameters);

        var result = Evaluate(module, collected, command.Language);
        if (result.IsComplete)
        {
            return result;
        }

        // First turn only: prefix the per-field question with the full required-fields list, so
        // the user knows up front what the module needs, without repeating that list on every
        // subsequent turn (ContinueAsync's Evaluate() call below never adds this prefix).
        var requiredNames = module.Fields.Where(f => f.IsRequired && !f.IsSystemGenerated && !f.IsCalculated).Select(f => f.DisplayName).ToList();
        result.NextQuestion = ShopAiPhrases.RequiredFieldsIntro(command.Language, module.DisplayName, requiredNames) + "\n\n" + result.NextQuestion;

        var pendingAction = _pendingActionManager.Create(userId, conversation.Id, sourceMessageId, module.CreateAction, module.ModuleKey, _clock.Now.Add(CollectingExpiry));
        _pendingActionManager.UpdateCollectedState(pendingAction, SerializeValues(collected), JsonSerializer.Serialize(result.MissingRequiredFields), "[]", _clock.Now.Add(CollectingExpiry));
        await _pendingActionRepository.InsertAsync(pendingAction, autoSave: true);

        result.PendingAction = pendingAction;
        return result;
    }

    public async Task<ShopAiSlotFillingResult> ContinueAsync(ShopAiPendingAction pendingAction, string userMessage, ShopAiLanguage language, CancellationToken cancellationToken = default)
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
        var missingBefore = Evaluate(module, collected, language).MissingRequiredFields;

        // Only required fields are ever asked about one at a time (see Evaluate() below), so
        // "skip" never legitimately applies here - this loop only extracts values for fields the
        // module actually needs before it can build a valid preview.
        var trimmed = userMessage.Trim();
        if (!string.IsNullOrWhiteSpace(trimmed) && missingBefore.Count > 0)
        {
            var targetFields = module.Fields.Where(f => missingBefore.Contains(f.FieldKey, StringComparer.OrdinalIgnoreCase)).ToList();
            var extracted = await _fieldExtractionService.ExtractAsync(module, targetFields, trimmed, cancellationToken);
            MergeProvidedValues(module, collected, extracted);
        }

        var evaluated = Evaluate(module, collected, language);
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

            if (field.DataType == "Boolean")
            {
                // Ollama is instructed to emit real JSON true/false, but a small local model
                // sometimes returns the word instead ("haan", "No") - coerce here so every
                // downstream consumer (handler DTOs, the preview, CollectedValuesJson) only ever
                // sees a real boolean, never a string it has to re-interpret itself.
                var parsed = ShopAiBooleanParser.TryParse(value);
                if (parsed == null) continue; // Unrecognized boolean phrase - treat as not provided rather than storing garbage.
                collected[field.FieldKey] = ShopAiBooleanParser.ToJsonElement(parsed.Value);
                continue;
            }

            collected[field.FieldKey] = value;
        }
    }

    private static ShopAiSlotFillingResult Evaluate(ShopAiModuleMetadata module, Dictionary<string, JsonElement> collected, ShopAiLanguage language)
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
            result.NextQuestion = ShopAiPhrases.AskField(language, nextField);
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

}
