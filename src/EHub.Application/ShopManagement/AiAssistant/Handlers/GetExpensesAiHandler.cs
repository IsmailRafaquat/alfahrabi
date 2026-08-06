using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Expenses;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>Read-only "list existing Expenses" - see GetUnitsAiHandler for the full pattern this mirrors.</summary>
public class GetExpensesAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopExpenseAppService _appService;

    public GetExpensesAiHandler(IShopAiCommandValidator validator, IShopExpenseAppService appService)
    {
        _validator = validator;
        _appService = appService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetExpenses;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryTransactionalData);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QueryExpenses);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopExpenses.Default);

        return new ShopAiActionPreviewDto { Action = ActionType, ActionDisplayNameKey = "::AiAction:GetExpenses", IsWriteAction = false, RequiresConfirmation = false };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var payload = ShopAiPayloadSerializer.Deserialize<GetExpensesAiCommand>(action.PayloadJson) ?? new GetExpensesAiCommand();
        var maxResultCount = Math.Clamp(payload.MaxResultCount <= 0 ? 100 : payload.MaxResultCount, 1, 500);

        var result = await _appService.GetListAsync(new GetShopExpensesInput
        {
            Filter = string.IsNullOrWhiteSpace(payload.Filter) ? null : payload.Filter.Trim(),
            MaxResultCount = maxResultCount,
            SkipCount = 0,
            Sorting = "ExpenseDate desc",
        });

        var rows = new List<Dictionary<string, object?>>();
        foreach (var item in result.Items)
        {
            rows.Add(new Dictionary<string, object?>
            {
                ["id"] = item.Id,
                ["expenseNumber"] = item.ExpenseNumber,
                ["expenseCategoryName"] = item.ExpenseCategoryName,
                ["expenseDate"] = item.ExpenseDate,
                ["amount"] = item.Amount,
                ["status"] = item.Status.ToString(),
            });
        }

        var message = ShopAiListResultMessageBuilder.Build(action.Language, "Expenses", "Expenses", "اخراجات", result.TotalCount);

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = message,
            DataList = new ShopAiDataListDto { Title = "Expenses", TotalCount = result.TotalCount, Rows = rows },
        };
    }
}
