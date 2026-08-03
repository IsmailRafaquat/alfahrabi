using System;
using System.Linq;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Deterministic safety net for the command parser, mirroring ShopAiCreationVerbDetector: a small
/// local model can misclassify a clear "list/count/search existing records" request as
/// ExplainModule/ListModuleFields/GeneralHelp, since those intents also sound plausible for a
/// message that names a module. If the raw message contains one of these read/list/count/search
/// phrases, ShopAiCommandParser treats that as a strong signal to prefer ReadBusinessData over
/// whatever weaker intent Ollama returned - see the intent-priority rule in ParseAsync. Phrases are
/// deliberately generic (no module name baked in) - the module itself is resolved separately via
/// IShopAiModuleMetadataProvider, the same way the creation-verb override resolves it.
/// </summary>
public static class ShopAiReadDataVerbDetector
{
    private static readonly string[] Phrases =
    {
        // English
        "show all", "list all", "how many", "are there", "in my system", "show existing",
        "get units", "get products", "get customers", "get suppliers", "display all",
        "show active", "show inactive", "search", "show me all", "existing ",

        // Roman Urdu
        "sab ", "ki list", "kitni ", "kitne ", "tamam ", "dikhao", "dikhaen", "dikha do",
        "record dikhao", "list show karo", "mein kitni", "mein kitne", "existing ",

        // Urdu
        "تمام", "کی فہرست", "کتنے", "کتنی", "موجودہ", "دکھائیں", "فعال", "غیر فعال",
    };

    public static bool ContainsReadDataVerb(string? text) =>
        !string.IsNullOrWhiteSpace(text) && Phrases.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase));
}
