using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Units;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>
/// Read-only "list existing Units" - answers "how many Units do I have / show all Units" style
/// requests. Distinct from ExplainModule/ListModuleFields (which describe what a Unit IS and what
/// fields it needs) - this queries the current tenant's actual ShopUnit records via the existing
/// IShopUnitAppService.GetListAsync, never a repository/DbContext directly, and never accepts
/// TenantId from the caller (ICurrentTenant is resolved inside ShopUnitAppService itself).
/// </summary>
public class GetUnitsAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopUnitAppService _unitAppService;

    public GetUnitsAiHandler(IShopAiCommandValidator validator, IShopUnitAppService unitAppService)
    {
        _validator = validator;
        _unitAppService = unitAppService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetUnits;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryMasterData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryUnits);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopUnits.Default);

        // Read action - executes immediately, no confirmation/preview fields needed.
        return new ShopAiActionPreviewDto
        {
            Action = ActionType,
            ActionDisplayNameKey = "::AiAction:GetUnits",
            IsWriteAction = false,
            RequiresConfirmation = false,
        };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetUnitsAiCommand>(action.PayloadJson) ?? new GetUnitsAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _unitAppService.GetListAsync(new GetShopUnitsInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            IsActive = payload.IncludeInactive ? null : true,
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "Name asc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var unit in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = unit.Id,
                ["name"] = unit.Name,
                ["shortName"] = unit.ShortName,
                ["allowDecimal"] = unit.AllowDecimal,
                ["isActive"] = unit.IsActive,
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Units", "Units", "یونٹس", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Units", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
