using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EHub.McpServer.Tools;

public class PrepareCreateUnitMcpInput
{
    [Required]
    [Description("The unit's full name, e.g. Piece, Kilogram, Meter.")]
    public string Name { get; set; } = default!;

    [Required]
    [Description("A short abbreviation shown throughout the app, e.g. Pc, Kg, M.")]
    public string ShortName { get; set; } = default!;

    [Required]
    [Description("Whether products in this unit can be sold/stocked in fractional amounts (e.g. 1.5). No for whole-count units like Piece; Yes for weight/length/volume units like Kilogram or Meter.")]
    public bool AllowDecimal { get; set; }

    [Description("Whether the unit is selectable for new products. Defaults to true.")]
    public bool IsActive { get; set; } = true;
}

public class PrepareCreateCustomerMcpInput
{
    [Required]
    [Description("The customer's display name.")]
    public string Name { get; set; } = default!;

    [Description("Phone number, if the user provided one.")]
    public string? Phone { get; set; }

    [Description("Email address, if the user provided one.")]
    public string? Email { get; set; }

    [Description("A contact person's name, if the user provided one.")]
    public string? ContactPerson { get; set; }
}

// ------------------------------------------------------------------
// Read-only "list existing records" tool inputs - never TenantId or UserId (both are resolved
// server-side from the authenticated request via ICurrentTenant/ICurrentUser).
// ------------------------------------------------------------------

public class GetUnitsMcpInput
{
    [Description("Optional text to search by Unit name or short name.")]
    public string? Filter { get; set; }

    [Description("Whether to include inactive Units. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;

    [Description("Maximum number of Units to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetProductCategoriesMcpInput
{
    [Description("Optional text to search by category name or code.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive categories. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of categories to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetProductsMcpInput
{
    [Description("Optional text to search by product name, code, SKU, or barcode.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive products. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of products to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetCustomersMcpInput
{
    [Description("Optional text to search by customer name, code, or phone.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive customers. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of customers to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetSuppliersMcpInput
{
    [Description("Optional text to search by supplier name, code, or phone.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive suppliers. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of suppliers to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetExpenseCategoriesMcpInput
{
    [Description("Optional text to search by category name or code.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive categories. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of categories to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetBankAccountsMcpInput
{
    [Description("Optional text to search by account name, bank name, or code.")]
    public string? Filter { get; set; }
    [Description("Whether to include inactive accounts. Defaults to true (show all).")]
    public bool IncludeInactive { get; set; } = true;
    [Description("Maximum number of accounts to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetSalesMcpInput
{
    [Description("Optional text to search by sale number or customer name.")]
    public string? Filter { get; set; }
    [Description("Maximum number of sales to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetPurchaseOrdersMcpInput
{
    [Description("Optional text to search by purchase order number or supplier name.")]
    public string? Filter { get; set; }
    [Description("Maximum number of purchase orders to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetExpensesMcpInput
{
    [Description("Optional text to search by expense number, category, or description.")]
    public string? Filter { get; set; }
    [Description("Maximum number of expenses to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetStockAdjustmentsMcpInput
{
    [Description("Optional text to search by adjustment number.")]
    public string? Filter { get; set; }
    [Description("Maximum number of stock adjustments to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

public class GetPhysicalStockCountsMcpInput
{
    [Description("Optional text to search by stock count number.")]
    public string? Filter { get; set; }
    [Description("Maximum number of stock counts to return (1-500). Defaults to 100.")]
    public int MaxResultCount { get; set; } = 100;
}

/// <summary>
/// Common confirm-tool input for every write action - deliberately carries no editable field
/// values. The server already has the validated payload stored against PendingActionId from the
/// matching prepare call; re-sending it here would let a caller substitute different values than
/// what was actually previewed and approved.
/// </summary>
public class ConfirmMcpActionInput
{
    [Required]
    [Description("The PendingActionId returned by the matching shop_prepare_* tool call.")]
    public Guid PendingActionId { get; set; }

    [Required]
    [Description("The ConfirmationToken returned by the matching shop_prepare_* tool call.")]
    public string ConfirmationToken { get; set; } = default!;
}
