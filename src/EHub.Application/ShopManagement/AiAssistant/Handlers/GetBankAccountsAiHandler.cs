using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Bank Accounts" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetBankAccountsAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopBankAccountAppService _appService;

    public GetBankAccountsAiHandler(IShopAiCommandValidator validator, IShopBankAccountAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetBankAccounts;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryMasterData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryBankAccounts);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopBankAccounts.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetBankAccounts", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetBankAccountsAiCommand>(action.PayloadJson) ?? new GetBankAccountsAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopBankAccountsInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            IsActive = payload.IncludeInactive ? null : true,
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "AccountName asc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["code"] = item.Code,
                ["accountName"] = item.AccountName,
                ["bankName"] = item.BankName,
                ["currentBalance"] = item.CurrentBalance,
                ["isActive"] = item.IsActive,
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Bank Accounts", "Bank Accounts", "بینک اکاؤنٹس", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Bank Accounts", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
