using System;
using System.Linq;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Deterministic safety net for the command parser: a small local model can misclassify a clear
/// creation request as ExplainModule/ListModuleFields/GeneralHelp, especially when it's written as
/// several short "field value" lines rather than one sentence. If the raw message contains one of
/// these explicit creation phrases, ShopAiCommandParser treats that as a strong signal to prefer
/// StartRecordCreation over whatever weaker intent Ollama returned - see the intent-priority rule
/// in ParseAsync.
/// </summary>
public static class ShopAiCreationVerbDetector
{
    private static readonly string[] Phrases =
    {
        // English - multi-word phrases first so a substring scan doesn't need ordering to matter,
        // but longer/more specific phrases are listed for clarity of intent even though Contains
        // matching itself is order-independent.
        "add new", "create new", "new entry", "add to the system",
        "add", "create", "make", "insert", "save",

        // Roman Urdu
        "add karo", "bana do", "banao", "create karo", "entry karo",
        "naya record banao", "system mein add karo", "yahan add karo",

        // Urdu
        "شامل کریں", "نیا ریکارڈ بنائیں", "اندراج کریں", "محفوظ کریں",
    };

    public static bool ContainsCreationVerb(string? text) =>
        !string.IsNullOrWhiteSpace(text) && Phrases.Any(p => text.Contains(p, StringComparison.OrdinalIgnoreCase));
}
