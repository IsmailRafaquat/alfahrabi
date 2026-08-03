namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>
/// Deterministic natural-language "N records found" / "none found" message in the user's detected
/// language, shared by every Get*AiHandler that returns a ShopAiDataListDto. Deliberately not left
/// to the model to phrase - the count must always match what the query actually returned.
/// </summary>
internal static class ShopAiListResultMessageBuilder
{
    public static string Build(ShopAiLanguage language, string englishPlural, string romanUrduPlural, string urduPlural, long count)
    {
        return language switch
        {
            ShopAiLanguage.RomanUrdu => count == 0
                ? $"Current shop mein koi {romanUrduPlural} nahi mili."
                : $"Aapke system mein total {count} {romanUrduPlural} hain.",
            ShopAiLanguage.Urdu => count == 0
                ? $"موجودہ شاپ میں کوئی {urduPlural} نہیں ملی۔"
                : $"آپ کے سسٹم میں کل {count} {urduPlural} موجود ہیں۔",
            _ => count == 0
                ? $"No {englishPlural} were found in the current shop."
                : $"There are {count} {englishPlural} in your system.",
        };
    }
}
