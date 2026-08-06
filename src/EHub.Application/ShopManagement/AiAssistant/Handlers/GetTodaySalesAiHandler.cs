using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.Dashboard;
using EHub.ShopManagement.Settings;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.AiAssistant.Handlers;

/// <summary>
/// Read-only. Reuses the existing ShopDashboardAppService.GetSummaryAsync (Period = Today) instead
/// of querying sales directly - all the "only completed sales count" / date-range logic already
/// lives there and is not duplicated here. Executes immediately; never requires confirmation.
/// </summary>
public class GetTodaySalesAiHandler : IShopAiActionHandler, ITransientDependency
{
    private readonly IShopAiCommandValidator _validator;
    private readonly IShopDashboardAppService _dashboardAppService;
    private readonly IShopSettingAppService _settingAppService;

    public GetTodaySalesAiHandler(
        IShopAiCommandValidator validator,
        IShopDashboardAppService dashboardAppService,
        IShopSettingAppService settingAppService)
    {
        _validator = validator;
        _dashboardAppService = dashboardAppService;
        _settingAppService = settingAppService;
    }

    public ShopAiActionType ActionType => ShopAiActionType.GetTodaySales;
    public bool IsWriteAction => false;

    public async Task<ShopAiActionPreviewDto> PrepareAsync(ShopAiParsedCommand command, CancellationToken cancellationToken = default)
    {
        _validator.RequireTenant();
        _validator.RequireUser();
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.Use);
        await _validator.EnsurePermissionAsync(EHubPermissions.ShopAiAssistant.QuerySales);

        // No parameters to resolve - this action always means "today". Nothing to confirm, so the
        // preview carries no fields and no confirmation token; the AppService executes it immediately.
        return new ShopAiActionPreviewDto
        {
            Action = ActionType,
            ActionDisplayNameKey = "::AiAction:GetTodaySales",
            IsWriteAction = false,
            RequiresConfirmation = false,
        };
    }

    public async Task<ShopAiExecutionResultDto> ExecuteAsync(ShopAiValidatedAction action, CancellationToken cancellationToken = default)
    {
        var summary = await _dashboardAppService.GetSummaryAsync(new GetShopDashboardInput { Period = ShopDashboardPeriod.Today });
        var settings = await SafeGetSettingsAsync();

        if (summary.NetSales == null || summary.TotalSalesCount == null)
        {
            // ShopDashboard.ViewFinancialSummary (the existing module permission) was not granted -
            // GetSummaryAsync silently omits these fields rather than throwing, so this handler must
            // treat "no data" as a valid, non-error outcome rather than a null-reference bug.
            return new ShopAiExecutionResultDto { Success = true, ResultMessage = "You don't have permission to view sales amounts." };
        }

        var currencySymbol = string.IsNullOrWhiteSpace(settings?.CurrencySymbol) ? settings?.CurrencyCode ?? string.Empty : settings.CurrencySymbol;
        var decimalPlaces = settings?.DecimalPlaces ?? 2;
        var netSalesText = summary.NetSales.Value.ToString("N" + decimalPlaces);

        var resultMessage = summary.TotalSalesCount > 0
            ? $"Today's net sales are {currencySymbol}{netSalesText} from {summary.TotalSalesCount} completed sale(s)."
            : "No completed sales recorded today yet.";

        var resultData = JsonSerializer.SerializeToElement(new
        {
            netSales = summary.NetSales,
            grossSales = summary.GrossSales,
            saleReturns = summary.SaleReturns,
            saleCount = summary.TotalSalesCount,
            currencyCode = settings?.CurrencyCode,
            currencySymbol,
        });

        return new ShopAiExecutionResultDto
        {
            Success = true,
            ResultMessage = resultMessage,
            ResultData = resultData,
        };
    }

    private async Task<ShopSettingDto?> SafeGetSettingsAsync()
    {
        try
        {
            return await _settingAppService.GetAsync();
        }
        catch
        {
            // Shop Settings may not be configured yet for a brand-new tenant - fall back to no currency
            // formatting rather than failing the whole read action over a missing settings row.
            return null;
        }
    }
}
