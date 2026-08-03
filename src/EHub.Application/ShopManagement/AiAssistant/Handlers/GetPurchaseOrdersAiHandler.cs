using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.PurchaseOrders;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Purchase Orders" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetPurchaseOrdersAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopPurchaseOrderAppService _appService;

    public GetPurchaseOrdersAiHandler(IShopAiCommandValidator validator, IShopPurchaseOrderAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetPurchaseOrders;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryTransactionalData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryPurchaseOrders);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopPurchaseOrders.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetPurchaseOrders", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetPurchaseOrdersAiCommand>(action.PayloadJson) ?? new GetPurchaseOrdersAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopPurchaseOrdersInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "OrderDate desc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["purchaseOrderNumber"] = item.PurchaseOrderNumber,
                ["supplierName"] = item.SupplierName,
                ["orderDate"] = item.OrderDate,
                ["grandTotal"] = item.GrandTotal,
                ["status"] = item.Status.ToString(),
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Purchase Orders", "Purchase Orders", "خریداری آرڈرز", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Purchase Orders", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
