using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Customers;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Customers" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetCustomersAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopCustomerAppService _appService;

    public GetCustomersAiHandler(IShopAiCommandValidator validator, IShopCustomerAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetCustomers;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryMasterData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryCustomers);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopCustomers.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetCustomers", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetCustomersAiCommand>(action.PayloadJson) ?? new GetCustomersAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopCustomersInput
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
                ["phone"] = item.Phone,
                ["openingBalance"] = item.OpeningBalance,
                ["isActive"] = item.IsActive,
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Customers", "Customers", "کسٹمرز", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Customers", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
