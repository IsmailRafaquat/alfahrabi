using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.StockAdjustments;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Stock Adjustments" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetStockAdjustmentsAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopStockAdjustmentAppService _appService;

    public GetStockAdjustmentsAiHandler(IShopAiCommandValidator validator, IShopStockAdjustmentAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetStockAdjustments;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryTransactionalData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryStockAdjustments);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopStockAdjustments.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetStockAdjustments", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetStockAdjustmentsAiCommand>(action.PayloadJson) ?? new GetStockAdjustmentsAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopStockAdjustmentsInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "AdjustmentDate desc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["adjustmentNumber"] = item.AdjustmentNumber,
                ["adjustmentDate"] = item.AdjustmentDate,
                ["reason"] = item.Reason.ToString(),
                ["status"] = item.Status.ToString(),
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Stock Adjustments", "Stock Adjustments", "اسٹاک ایڈجسٹمنٹس", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Stock Adjustments", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
