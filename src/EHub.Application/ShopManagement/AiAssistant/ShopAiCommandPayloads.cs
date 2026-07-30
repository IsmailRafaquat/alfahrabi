using System;
using System.Collections.Generic;

namespace EHub.ShopManagement.AiAssistant;

// Strongly-typed payloads that ShopAiCommandValidator deserializes ShopAiParsedCommand.Parameters
// into (one type per supported action) before any value is trusted. Deliberately no Id fields
// anywhere here - Ollama is only ever allowed to supply human-readable names/codes; the validator
// resolves those into current-tenant IDs via ShopAiLookupResolver. A handler must never execute
// from the raw Dictionary<string, JsonElement> on ShopAiParsedCommand directly.

public class CreateCustomerAiCommand
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
}

public class CreateSupplierAiCommand
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
}

public class CreateProductDraftAiCommand
{
    public string? Name { get; set; }
    public string? CategoryName { get; set; }
    public string? UnitName { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? SalePrice { get; set; }
    public string? Sku { get; set; }
}

public class CreateExpenseDraftAiCommand
{
    public string? CategoryName { get; set; }
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpenseDate { get; set; }
}

public class CreatePurchaseOrderDraftItemAiCommand
{
    public string? ProductName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class CreatePurchaseOrderDraftAiCommand
{
    public string? SupplierName { get; set; }
    public List<CreatePurchaseOrderDraftItemAiCommand> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CreateSaleDraftItemAiCommand
{
    public string? ProductName { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

public class CreateSaleDraftAiCommand
{
    public string? CustomerName { get; set; }
    public List<CreateSaleDraftItemAiCommand> Items { get; set; } = new();
    public string? Notes { get; set; }
}

public class CustomerBalanceQueryAiCommand
{
    public string? CustomerName { get; set; }
}

public class SupplierBalanceQueryAiCommand
{
    public string? SupplierName { get; set; }
}

public class ProductStockQueryAiCommand
{
    public string? ProductName { get; set; }
}

public class SalesSummaryQueryAiCommand
{
    /// <summary>Free-text period hint from the model, e.g. "today", "this week" - re-validated against ShopDashboardPeriod, never trusted as-is.</summary>
    public string? Period { get; set; }
}
