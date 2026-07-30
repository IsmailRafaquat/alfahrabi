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

    public ShopAiLanguage Language { get; set; }

    public bool RequiresConfirmation { get; set; }

    public decimal Confidence { get; set; }

    public string? UserFriendlyMessage { get; set; }

    public Dictionary<string, JsonElement> Parameters { get; set; } = new();

    public List<string> MissingFields { get; set; } = new();

    public List<string> Warnings { get; set; } = new();
}
