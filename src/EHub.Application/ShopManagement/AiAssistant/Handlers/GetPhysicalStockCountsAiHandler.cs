using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.StockCounts;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Physical Stock Counts" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetPhysicalStockCountsAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopStockCountAppService _appService;

    public GetPhysicalStockCountsAiHandler(IShopAiCommandValidator validator, IShopStockCountAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetPhysicalStockCounts;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryTransactionalData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryPhysicalStockCounts);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopStockCounts.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetPhysicalStockCounts", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetPhysicalStockCountsAiCommand>(action.PayloadJson) ?? new GetPhysicalStockCountsAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopStockCountsInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "CountDate desc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["stockCountNumber"] = item.StockCountNumber,
                ["countDate"] = item.CountDate,
                ["status"] = item.Status.ToString(),
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Physical Stock Counts", "Physical Stock Counts", "اسٹاک کاؤنٹس", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Physical Stock Counts", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
