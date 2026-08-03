using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.ProductCategories;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Product Categories" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetProductCategoriesAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopProductCategoryAppService _appService;

    public GetProductCategoriesAiHandler(IShopAiCommandValidator validator, IShopProductCategoryAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetProductCategories;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryMasterData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryProductCategories);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopProductCategories.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetProductCategories", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetProductCategoriesAiCommand>(action.PayloadJson) ?? new GetProductCategoriesAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopProductCategoriesInput
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
                ["name"] = item.Name,
                ["code"] = item.Code,
                ["parentCategoryName"] = item.ParentCategoryName,
                ["isActive"] = item.IsActive,
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Product Categories", "Product Categories", "پروڈکٹ کیٹیگریز", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Product Categories", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
