using System.Collections.Generic;
using System.Text.Json;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// The raw shape of whatever Ollama returned, deserialized but NOT yet trusted. Every field here is
/// untrusted input: Action must be checked against the handler allowlist, Parameters must be
/// deserialized into the matching strongly-typed *AiCommand and validated field-by-field, and no
/// value (especially not an ID) may be used until ShopAiCommandValidator has run.
/// </summary>
public class ShopAiParsedCommand
{
    public ShopAiActionType Action { get; set; }

    /// <summary>What the user actually wants to do - GeneralHelp/ExplainModule/ListModuleFields/
    /// ExplainField/StartRecordCreation/ReadBusinessData/etc. Action above stays meaningful only for
    /// ReadBusinessData (one of the Get* actions) and StartRecordCreation (one of the Create*
    /// actions); for the knowledge-only intents Action is Unknown and ModuleKey/FieldKey carry the
    /// real information.</summary>
    public ShopAiIntentType Intent { get; set; } = ShopAiIntentType.Unknown;

    /// <summary>Set when Intent is ExplainModule/ListModuleFields/ExplainField/StartRecordCreation - must match a key or alias registered in IShopAiModuleMetadataProvider, never invented.</summary>
    public string? ModuleKey { get; set; }

    /// <summary>Set when Intent is ExplainField - must match a FieldKey registered on the resolved module.</summary>
    public string? FieldKey { get; set; }

    public ShopAiLanguage Language { get; set; }

    public bool RequiresConfirmation { get; set; }

    public decimal Confidence { get; set; }

    public string? UserFriendlyMessage { get; set; }

    public Dictionary<string, JsonElement> Parameters { get; set; } = new();

    public List<string> MissingFields { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
}
