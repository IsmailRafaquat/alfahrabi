using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Dashboard;

public interface IShopDashboardAppService : IApplicationService
{
    Task<ShopDashboardDto> GetAsync(GetShopDashboardInput input);

    Task<ShopDashboardSummaryDto> GetSummaryAsync(GetShopDashboardInput input);

    Task<ShopDashboardSalesExpenseChartDto> GetSalesExpenseChartAsync(GetShopDashboardInput input);

    Task<ListResultDto<ShopDashboardTopProductDto>> GetTopProductsAsync(GetShopDashboardInput input);

    Task<ListResultDto<ShopDashboardLowStockProductDto>> GetLowStockProductsAsync();

    Task<ListResultDto<ShopDashboardBatchAlertDto>> GetNearExpiryBatchesAsync(int maxResultCount = 10);

    Task<ListResultDto<ShopDashboardBatchAlertDto>> GetExpiredBatchesAsync(int maxResultCount = 10);

    Task<ShopDashboardBalanceSummaryDto> GetBalancesAsync();

    Task<ListResultDto<ShopDashboardRecentSaleDto>> GetRecentSalesAsync(int maxResultCount = 5);

    Task<ListResultDto<ShopDashboardRecentPurchaseDto>> GetRecentPurchasesAsync(int maxResultCount = 5);

    Task<ListResultDto<ShopDashboardRecentExpenseDto>> GetRecentExpensesAsync(int maxResultCount = 5);
}
