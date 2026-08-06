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
/// Prepare/confirm pair for Unit creation - the reference implementation every other write-tool
/// pair in this project follows. Neither method touches ShopUnit, its repository, or DbContext:
/// both go through IShopAiActionExecutor, which itself only ever calls the existing
/// CreateUnitAiHandler -> IShopUnitAppService -> ShopUnitManager chain.
/// </summary>
[McpServerToolType]
public class UnitMcpTools
{
    private readonly IShopAiActionExecutor _executor;
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopAiRateLimiter _rateLimiter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public UnitMcpTools(IShopAiActionExecutor executor, IShopAiCommandValidator validator, IShopAiRateLimiter rateLimiter, ICurrentTenant currentTenant, ICurrentUser currentUser)
    {
        _executor = executor;
        _validator = validator;
        _rateLimiter = rateLimiter;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    private void EnsureNotRateLimited(ShopAiRateLimitCategory category)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
        var userId = _currentUser.Id ?? throw new BusinessException("ShopManagement:AiPermissionDenied");
        if (!_rateLimiter.TryAcquire(tenantId, userId, category))
        {
            throw new BusinessException("ShopManagement:AiRateLimitExceeded");
        }
    }

    [McpServerTool(Name = "shop_prepare_create_unit")]
    [Description(
        "Validates Unit details and prepares a confirmation preview. Use this tool when the user " +
        "wants to create a Unit and has supplied Name, Short Name, and the Allow Decimal setting. " +
        "This tool does NOT create the Unit - it only validates the data and returns a preview. " +
        "The caller must show the preview to the user and only call shop_confirm_create_unit after " +
        "the user explicitly confirms.")]
    public async Task<ShopMcpToolResultDto<object?>> PrepareCreateUnitAsync(PrepareCreateUnitMcpInput input, CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.WriteTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.PrepareCreateUnit);
        // The handler itself also re-checks ShopAiAssistant.GuidedCreation, ShopAiAssistant.CreateUnit,
        // and the existing ShopUnits.Create permission - see CreateUnitAiHandler.PrepareAsync.
        EnsureNotRateLimited(ShopAiRateLimitCategory.McpPrepare);

        var payload = JsonSerializer.SerializeToElement(new
        {
            name = input.Name,
            shortName = input.ShortName,
            allowDecimal = input.AllowDecimal,
            isActive = input.IsActive,
        });

        try
        {
            var prepared = await _executor.PrepareAsync(ShopAiActionType.CreateUnit, payload, cancellationToken);
            var result = ShopMcpToolResultDto<object?>.Ok(null, prepared.Preview.ActionDisplayNameKey);
            result.RequiresConfirmation = true;
            result.ConversationId = prepared.ConversationId;
            result.MessageId = prepared.MessageId;
            result.PendingActionId = prepared.PendingActionId;
            result.ConfirmationToken = prepared.Preview.ConfirmationToken;
            result.ConfirmationExpiryDate = prepared.Preview.ConfirmationExpiryDate;
            result.Data = prepared.Preview.Fields;
            return result;
        }
        catch (ShopAiMissingInformationException ex)
        {
            return ShopMcpToolResultDto<object?>.Fail("AiMissingInformation", "Missing: " + string.Join(", ", ex.MissingFields));
        }
        catch (Volo.Abp.BusinessException ex)
        {
            return ShopMcpToolResultDto<object?>.Fail(ex.Code ?? "AiServiceUnavailable", ex.Message);
        }
    }

    [McpServerTool(Name = "shop_confirm_create_unit")]
    [Description(
        "Confirms and creates the Unit previously validated by shop_prepare_create_unit. " +
        "Only call this after the user has explicitly approved the preview. Uses the server-stored " +
        "validated payload - the Unit's field values are never sent again here.")]
    public async Task<ShopMcpToolResultDto<object?>> ConfirmCreateUnitAsync(ConfirmMcpActionInput input, CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.WriteTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.ConfirmCreateUnit);
        EnsureNotRateLimited(ShopAiRateLimitCategory.McpConfirm);

        var result = await _executor.ConfirmAsync(input.PendingActionId, input.ConfirmationToken, cancellationToken);
        return result.Success
            ? ShopMcpToolResultDto<object?>.Ok(new { unitId = result.ResultReferenceId }, result.ResultMessage ?? "Unit created.")
            : ShopMcpToolResultDto<object?>.Fail(result.ErrorCode ?? "AiServiceUnavailable", result.ErrorMessage ?? "Unable to create the Unit.");
    }
}
