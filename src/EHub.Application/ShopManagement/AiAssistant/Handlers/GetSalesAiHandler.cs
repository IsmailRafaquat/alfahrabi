using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Sales;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Sales" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetSalesAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopSaleAppService _appService;

    public GetSalesAiHandler(IShopAiCommandValidator validator, IShopSaleAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetSales;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryTransactionalData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QuerySales);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopSales.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetSales", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetSalesAiCommand>(action.PayloadJson) ?? new GetSalesAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopSalesInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "SaleDate desc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["saleNumber"] = item.SaleNumber,
                ["customerName"] = item.CustomerName,
                ["saleDate"] = item.SaleDate,
                ["grandTotal"] = item.GrandTotal,
                ["status"] = item.Status.ToString(),
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Sales", "Sales", "سیلز", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Sales", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
