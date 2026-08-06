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

[McpServerToolType]
public class CustomerMcpTools
{
    private readonly IShopAiActionExecutor _executor;
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopAiRateLimiter _rateLimiter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public CustomerMcpTools(IShopAiActionExecutor executor, IShopAiCommandValidator validator, IShopAiRateLimiter rateLimiter, ICurrentTenant currentTenant, ICurrentUser currentUser)
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

    [McpServerTool(Name = "shop_prepare_create_customer")]
    [Description(
        "Validates Customer details and prepares a confirmation preview. Use this tool when the " +
        "user wants to create a Customer and has supplied at least a Name. This tool does NOT " +
        "create the Customer - it only validates the data and returns a preview.")]
    public async Task<ShopMcpToolResultDto<object?>> PrepareCreateCustomerAsync(PrepareCreateCustomerMcpInput input, CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.WriteTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.PrepareCreateCustomer);
        EnsureNotRateLimited(ShopAiRateLimitCategory.McpPrepare);

        var payload = JsonSerializer.SerializeToElement(new
        {
            name = input.Name,
            phone = input.Phone,
            email = input.Email,
            contactPerson = input.ContactPerson,
        });

        try
        {
            var prepared = await _executor.PrepareAsync(ShopAiActionType.CreateCustomer, payload, cancellationToken);
            var result = ShopMcpToolResultDto<object?>.Ok(prepared.Preview.Fields, prepared.Preview.ActionDisplayNameKey);
            result.RequiresConfirmation = true;
            result.ConversationId = prepared.ConversationId;
            result.MessageId = prepared.MessageId;
            result.PendingActionId = prepared.PendingActionId;
            result.ConfirmationToken = prepared.Preview.ConfirmationToken;
            result.ConfirmationExpiryDate = prepared.Preview.ConfirmationExpiryDate;
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

    [McpServerTool(Name = "shop_confirm_create_customer")]
    [Description("Confirms and creates the Customer previously validated by shop_prepare_create_customer.")]
    public async Task<ShopMcpToolResultDto<object?>> ConfirmCreateCustomerAsync(ConfirmMcpActionInput input, CancellationToken cancellationToken)
    {
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.WriteTools);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopMcp.ConfirmCreateCustomer);
        EnsureNotRateLimited(ShopAiRateLimitCategory.McpConfirm);

        var result = await _executor.ConfirmAsync(input.PendingActionId, input.ConfirmationToken, cancellationToken);
        return result.Success
            ? ShopMcpToolResultDto<object?>.Ok(new { customerId = result.ResultReferenceId }, result.ResultMessage ?? "Customer created.")
            : ShopMcpToolResultDto<object?>.Fail(result.ErrorCode ?? "AiServiceUnavailable", result.ErrorMessage ?? "Unable to create the Customer.");
    }
}
