using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Products;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Products" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetProductsAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopProductAppService _appService;

    public GetProductsAiHandler(IShopAiCommandValidator validator, IShopProductAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetProducts;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryMasterData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryProducts);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopProducts.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetProducts", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetProductsAiCommand>(action.PayloadJson) ?? new GetProductsAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopProductsInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            IsActive = payload.IncludeInactive ? null : true,
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "Name asc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["code"] = item.Code,
                ["name"] = item.Name,
                ["categoryName"] = item.CategoryName,
                ["unitShortName"] = item.UnitShortName,
                ["currentStock"] = item.CurrentStock,
                ["salePrice"] = item.SalePrice,
                ["isActive"] = item.IsActive,
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Products", "Products", "پروڈکٹس", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Products", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
