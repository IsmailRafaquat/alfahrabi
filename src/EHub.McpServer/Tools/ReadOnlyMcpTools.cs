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
