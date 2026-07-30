using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>
/// Write action - always requires confirmation. Reachable either directly (a single message that
/// already has Name/ShortName/AllowDecimal) or via ShopAiSlotFillingService after a multi-turn
/// collection - both paths converge on the same PrepareAsync/ExecuteAsync here. Delegates the
/// actual insert to the existing IShopUnitAppService.CreateAsync, which owns name-uniqueness
/// checking via ShopUnitManager; this handler never touches ShopUnit or its repository directly.
/// </summary>
public class CreateUnitAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopUnitAppService _unitAppService;

    public CreateUnitAiHandler(IShopAiCommandValidator validator, IShopUnitAppService unitAppService)
    {
        _validator = validator;
        _unitAppService = unitAppService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.CreateUnit;
    public bool IsWriteAction => true;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.GuidedCreation);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.CreateUnit);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopUnits.Create);

        var payload = ShopAiPayloadSerializer.Deserialize<CreateUnitAiCommand>(command.Parameters) ?? new CreateUnitAiCommand();
        var name = payload.Name?.Trim();
        var shortName = payload.ShortName?.Trim();

        var missingFields = new List<string>();
        if (string.IsNullOrWhiteSpace(name)) missingFields.Add("name");
        if (string.IsNullOrWhiteSpace(shortName)) missingFields.Add("shortName");
        if (payload.AllowDecimal == null) missingFields.Add("allowDecimal");
        if (missingFields.Count > 0)
        {
            throw new ShopAiMissingInformationException(missingFields, command.UserFriendlyMessage);
        }

        return new ShopAiActionPreviewDto
        {
            Action = ActionType,
            ActionDisplayNameKey = "::AiAction:CreateUnit",
            IsWriteAction = true,
            RequiresConfirmation = true,
            Fields = new List<ShopAiPreviewFieldDto>
            {
                new() { LabelKey = "::Name", Value = name, IsEmpty = false },
                new() { LabelKey = "::ShortName", Value = shortName, IsEmpty = false },
                new() { LabelKey = "::AllowDecimalQuantity", Value = (payload.AllowDecimal == true) ? "Yes" : "No", IsEmpty = false },
            },
        };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<CreateUnitAiCommand>(action.PayloadJson) ?? new CreateUnitAiCommand();
        var name = payload.Name?.Trim();
        var shortName = payload.ShortName?.Trim();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(shortName) || payload.AllowDecimal == null)
        {
            return new ShopAiExecutionResultDto { Success = false, ErrorCode = "AiMissingInformation", ErrorMessage = "ShopManagement:AiMissingInformation" };
        }

        var dto = new CreateUpdateShopUnitDto
        {
            Name = name,
            ShortName = shortName,
            AllowDecimal = payload.AllowDecimal.Value,
            IsActive = true,
        };

        try
        {
            var created = await _unitAppService.CreateAsync(dto);
            return new ShopAiExecutionResultDto
            {
                Success = true,
                ResultMessage = $"Unit '{created.Name}' ({created.ShortName}) was created.",
                ResultReferenceType = "ShopUnit",
                ResultReferenceId = created.Id,
                ResultData = JsonSerializer.SerializeToElement(new { unitId = created.Id, name = created.Name, shortName = created.ShortName }),
            };
        }
        catch (BusinessException ex)
        {
            return new ShopAiExecutionResultDto { Success = false, ErrorCode = ex.Code, ErrorMessage = ex.Code };
        }
    }
}

public class CreateUnitAiCommand
{
    public string? Name { get; set; }
    public string? ShortName { get; set; }
    public bool? AllowDecimal { get; set; }
}
