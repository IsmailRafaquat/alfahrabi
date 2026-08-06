using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.AiAssistant;
using ModelContextProtocol.Server;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.McpServer.Tools;

/// <summary>
/// Read-only tools execute immediately (no prepare/confirm) but still enforce the same permission
/// layering every write tool uses: main MCP permission, category permission, tool-specific
/// permission, and (indirectly, via the handler itself) the existing module read permission.
/// </summary>
[McpServerToolType]
public class ReadOnlyMcpTools
{
    private readonly IShopAiActionExecutor _executor;
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopAiModuleMetadataProvider _moduleMetadataProvider;
    private readonly IShopAiRateLimiter _rateLimiter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ReadOnlyMcpTools(
        IShopAiActionExecutor executor,
        IShopAiCommandValidator validator,
        IShopAiModuleMetadataProvider moduleMetadataProvider,
        IShopAiRateLimiter rateLimiter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _executor = executor;
        _validator = validator;
        _moduleMetadataProvider = moduleMetadataProvider;
        _rateLimiter = rateLimiter;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    private void EnsureNotRateLimited()
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
        var userId = _currentUser.Id ?? throw new BusinessException("ShopManagement:AiPermissionDenied");
        if (!_rateLimiter.TryAcquire(tenantId, userId, ShopAiRateLimitCategory.McpRead))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }
    }

    [McpServerTool(Name = "shop_get_today_sales")]
    [Description("Returns a summary of today's completed sales for the current shop (net sales total and completed sale count). Read-only, executes immediately.")]
    public async Task<ShopMcpToolResultDto<JsonElement?>> GetTodaySalesAsync(CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.ReadTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.GetTodaySales);
        EnsureNotRateLimited();

        var result = await _executor.ExecuteReadAsync(ShopAiActionType.GetTodaySales, default, cancellationToken);
        return result.Success
            ? ShopMcpToolResultDto<JsonElement?>.Ok(result.ResultData, result.ResultMessage ?? string.Empty)
            : ShopMcpToolResultDto<JsonElement?>.Fail(result.ErrorCode ?? "AiServiceUnavailable", result.ErrorMessage ?? "Unable to retrieve today's sales.");
    }

    [McpServerTool(Name = "shop_get_units")]
    [Description(
        "Returns existing Shop Units for the authenticated current tenant. Use this tool when the " +
        "user asks: how many Units exist, to list all Units, to show active or inactive Units, or " +
        "to search Units. This tool returns existing records. It does not explain Unit fields and " +
        "does not create a Unit.")]
    public Task<ShopMcpToolResultDto<object?>> GetUnitsAsync(GetUnitsMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetUnits, EHubPermissions.ShopMcp.GetUnits, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Units", cancellationToken);

    [McpServerTool(Name = "shop_get_product_categories")]
    [Description("Returns existing Product Categories for the current tenant. Use for list/count/search requests - not for explaining what a category is.")]
    public Task<ShopMcpToolResultDto<object?>> GetProductCategoriesAsync(GetProductCategoriesMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetProductCategories, EHubPermissions.ShopMcp.GetProductCategories, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Product Categories", cancellationToken);

    [McpServerTool(Name = "shop_get_products")]
    [Description("Returns existing Products for the current tenant. Use for list/count/search requests - not for explaining what a product is.")]
    public Task<ShopMcpToolResultDto<object?>> GetProductsAsync(GetProductsMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetProducts, EHubPermissions.ShopMcp.GetProducts, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Products", cancellationToken);

    [McpServerTool(Name = "shop_get_customers")]
    [Description("Returns existing Customers for the current tenant. Use for list/count/search requests - not for explaining what a customer is.")]
    public Task<ShopMcpToolResultDto<object?>> GetCustomersAsync(GetCustomersMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetCustomers, EHubPermissions.ShopMcp.GetCustomers, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Customers", cancellationToken);

    [McpServerTool(Name = "shop_get_suppliers")]
    [Description("Returns existing Suppliers for the current tenant. Use for list/count/search requests - not for explaining what a supplier is.")]
    public Task<ShopMcpToolResultDto<object?>> GetSuppliersAsync(GetSuppliersMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetSuppliers, EHubPermissions.ShopMcp.GetSuppliers, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Suppliers", cancellationToken);

    [McpServerTool(Name = "shop_get_expense_categories")]
    [Description("Returns existing Expense Categories for the current tenant. Use for list/count/search requests - not for explaining what a category is.")]
    public Task<ShopMcpToolResultDto<object?>> GetExpenseCategoriesAsync(GetExpenseCategoriesMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetExpenseCategories, EHubPermissions.ShopMcp.GetExpenseCategories, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Expense Categories", cancellationToken);

    [McpServerTool(Name = "shop_get_bank_accounts")]
    [Description("Returns existing Bank Accounts for the current tenant. Use for list/count/search requests - not for explaining what a bank account is.")]
    public Task<ShopMcpToolResultDto<object?>> GetBankAccountsAsync(GetBankAccountsMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetBankAccounts, EHubPermissions.ShopMcp.GetBankAccounts, new { filter = input.Filter, includeInactive = input.IncludeInactive, maxResultCount = input.MaxResultCount }, "Bank Accounts", cancellationToken);

    [McpServerTool(Name = "shop_get_sales")]
    [Description("Returns existing Sales for the current tenant. Use for list/count/search requests - not for explaining the Sales module.")]
    public Task<ShopMcpToolResultDto<object?>> GetSalesAsync(GetSalesMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetSales, EHubPermissions.ShopMcp.GetSales, new { filter = input.Filter, maxResultCount = input.MaxResultCount }, "Sales", cancellationToken);

    [McpServerTool(Name = "shop_get_purchase_orders")]
    [Description("Returns existing Purchase Orders for the current tenant. Use for list/count/search requests - not for explaining what a purchase order is.")]
    public Task<ShopMcpToolResultDto<object?>> GetPurchaseOrdersAsync(GetPurchaseOrdersMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetPurchaseOrders, EHubPermissions.ShopMcp.GetPurchaseOrders, new { filter = input.Filter, maxResultCount = input.MaxResultCount }, "Purchase Orders", cancellationToken);

    [McpServerTool(Name = "shop_get_expenses")]
    [Description("Returns existing Expenses for the current tenant. Use for list/count/search requests - not for explaining what an expense is.")]
    public Task<ShopMcpToolResultDto<object?>> GetExpensesAsync(GetExpensesMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetExpenses, EHubPermissions.ShopMcp.GetExpenses, new { filter = input.Filter, maxResultCount = input.MaxResultCount }, "Expenses", cancellationToken);

    [McpServerTool(Name = "shop_get_stock_adjustments")]
    [Description("Returns existing Stock Adjustments for the current tenant. Use for list/count/search requests - not for explaining what a stock adjustment is.")]
    public Task<ShopMcpToolResultDto<object?>> GetStockAdjustmentsAsync(GetStockAdjustmentsMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetStockAdjustments, EHubPermissions.ShopMcp.GetStockAdjustments, new { filter = input.Filter, maxResultCount = input.MaxResultCount }, "Stock Adjustments", cancellationToken);

    [McpServerTool(Name = "shop_get_physical_stock_counts")]
    [Description("Returns existing Physical Stock Counts for the current tenant. Use for list/count/search requests - not for explaining what a stock count is.")]
    public Task<ShopMcpToolResultDto<object?>> GetPhysicalStockCountsAsync(GetPhysicalStockCountsMcpInput input, CancellationToken cancellationToken) =>
        ExecuteListAsync(ShopAiActionType.GetPhysicalStockCounts, EHubPermissions.ShopMcp.GetPhysicalStockCounts, new { filter = input.Filter, maxResultCount = input.MaxResultCount }, "Physical Stock Counts", cancellationToken);

    /// <summary>Shared execution path for every shop_get_* list tool - always ExecuteReadAsync (never PrepareAsync/ConfirmAsync, these never write), always reshapes ShopAiDataListDto into the { totalCount, items } shape every list tool returns.</summary>
    private async Task<ShopMcpToolResultDto<object?>> ExecuteListAsync(ShopAiActionType actionType, string toolPermission, object typedPayload, string recordNamePlural, CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.ReadTools);
        await _validator.EnsurePermissionAsync(toolPermission);
        EnsureNotRateLimited();

        var payload = JsonSerializer.SerializeToElement(typedPayload);
        var result = await _executor.ExecuteReadAsync(actionType, payload, cancellationToken);

        if (!result.Success)
        {
            return ShopMcpToolResultDto<object?>.Fail(result.ErrorCode ?? "AiServiceUnavailable", result.ErrorMessage ?? $"Unable to retrieve {recordNamePlural}.");
        }

        object? data = result.DataList == null ? null : new { totalCount = result.DataList.TotalCount, items = result.DataList.Rows };
        return ShopMcpToolResultDto<object?>.Ok(data, result.ResultMessage ?? $"{result.DataList?.TotalCount ?? 0} {recordNamePlural} found.");
    }

    [McpServerTool(Name = "shop_get_module_help")]
    [Description("Returns a description of a Shop Management module: what it does, required/optional fields, business rules, and whether the AI Assistant can create this record. Use the module key from shop://metadata/modules.")]
    public Task<ShopMcpToolResultDto<ShopAiModuleExplanationDto?>> GetModuleHelpAsync(
        [Description("The module key, e.g. \"unit\", \"customer\", \"productcategory\".")] string moduleKey,
        CancellationToken cancellationToken)
    {
        return GetModuleHelpInternalAsync(moduleKey);
    }

    private async Task<ShopMcpToolResultDto<ShopAiModuleExplanationDto?>> GetModuleHelpInternalAsync(string moduleKey)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.ReadTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.GetModuleHelp);
        EnsureNotRateLimited();

        if (!_moduleMetadataProvider.TryGetModule(moduleKey, out var module) || module == null)
        {
            return ShopMcpToolResultDto<ShopAiModuleExplanationDto?>.Fail("AiModuleNotFound", $"No module found for key '{moduleKey}'.");
        }

        var dto = new ShopAiModuleExplanationDto
        {
            ModuleKey = module.ModuleKey,
            DisplayName = module.DisplayName,
            Description = module.Description,
            SupportsCreation = module.SupportsCreation,
            CreatesDraftOnly = module.CreatesDraftOnly,
            BusinessRules = module.BusinessRules,
            RelatedModules = module.RelatedModules,
        };
        foreach (var field in module.Fields)
        {
            var fieldDto = new ShopAiFieldDescriptionDto
            {
                FieldKey = field.FieldKey,
                DisplayName = field.DisplayName,
                Description = field.Description,
                DataType = field.DataType,
                IsRequired = field.IsRequired,
                IsLookup = field.IsLookup,
                IsSystemGenerated = field.IsSystemGenerated,
                ExampleValue = field.ExampleValue,
                AllowedValues = field.AllowedValues,
            };
            if (field.IsSystemGenerated || field.IsCalculated) dto.SystemGeneratedFields.Add(fieldDto);
            else if (field.IsRequired) dto.RequiredFields.Add(fieldDto);
            else dto.OptionalFields.Add(fieldDto);
        }

        return ShopMcpToolResultDto<ShopAiModuleExplanationDto?>.Ok(dto, $"{module.DisplayName}: {module.Description}");
    }
}
