using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Structured "existing records" result for a ReadBusinessData list/count action (GetUnits,
/// GetProducts, etc.) - rendered by the frontend as a data-list card/table, never the module-help
/// (ExplainModule/ListModuleFields) card. Rows are pre-formatted, display-ready values only - never
/// TenantId, audit fields, soft-delete fields, or other internal metadata.
/// </summary>
public class ShopAiDataListDto
{
    public string Title { get; set; } = default!;

    public long TotalCount { get; set; }

    public List<Dictionary<string, object?>> Rows { get; set; } = new();
}
