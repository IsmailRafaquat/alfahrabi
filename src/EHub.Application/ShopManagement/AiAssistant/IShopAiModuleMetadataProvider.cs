using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

public interface IShopAiModuleMetadataProvider
{
    ShopAiModuleMetadata GetModule(string moduleKey);

    IReadOnlyList<ShopAiModuleMetadata> GetModules();

    bool TryGetModule(string moduleNameOrAlias, out ShopAiModuleMetadata? metadata);

    /// <summary>
    /// Unlike TryGetModule (exact key/alias match), this scans an arbitrary free-text message for
    /// any registered moduleKey/DisplayName/alias appearing as a substring, preferring the longest
    /// match. Used as a fallback when Ollama's own moduleKey guess is missing or wrong - e.g. the
    /// creation-verb safety net in ShopAiCommandParser needs to resolve a module even when Ollama
    /// misclassified the whole message's intent and never populated moduleKey at all.
    /// </summary>
    bool TryFindModuleInText(string text, out ShopAiModuleMetadata? metadata);
}
