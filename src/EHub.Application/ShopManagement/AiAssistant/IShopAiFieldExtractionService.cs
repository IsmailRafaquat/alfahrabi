using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Focused per-field extraction, shared by ShopAiSlotFillingService (extracting only the fields
/// still missing during a multi-turn continuation) and ShopAiCommandParser (backfilling a
/// module's fields in one shot when the deterministic creation-verb override fires on a message
/// the main classification prompt misclassified). Deliberately NOT the big command-parsing prompt
/// - a short, field-scoped prompt is both faster and less likely to invent values for fields
/// nobody mentioned.
/// </summary>
public interface IShopAiFieldExtractionService
{
    Task<Dictionary<string, JsonElement>> ExtractAsync(
        ShopAiModuleMetadata module,
        IReadOnlyList<ShopAiFieldMetadata> targetFields,
        string userMessage,
        CancellationToken cancellationToken = default);
}
