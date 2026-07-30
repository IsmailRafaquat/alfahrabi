using System;
using System.Text.Json;

namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Outcome of an action that actually ran (a read query executed immediately, or a write action
/// executed after confirmation). ResultData is a small structured payload for read actions (e.g.
/// { netSales: 45000, saleCount: 12 }) that a result card can render alongside ResultMessage.
/// </summary>
public class ShopAiExecutionResultDto
{
    public bool Success { get; set; }

    /// <summary>Human-readable summary, e.g. "Today's net sales are 45,000 from 12 completed sales.".</summary>
    public string? ResultMessage { get; set; }

    public JsonElement? ResultData { get; set; }

    /// <summary>e.g. "ShopCustomer" - set only for write actions that created something.</summary>
    public string? ResultReferenceType { get; set; }
    public Guid? ResultReferenceId { get; set; }

    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
