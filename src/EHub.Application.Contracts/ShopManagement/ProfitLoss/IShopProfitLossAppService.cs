using System.Threading.Tasks;
using EHub.ShopManagement.Reports;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.ShopManagement.ProfitLoss;

public interface IShopProfitLossAppService : IApplicationService
{
    Task<ShopProfitLossDto> GetAsync(GetShopProfitLossInput input);

    Task<ShopProfitLossSummaryDto> GetSummaryAsync(GetShopProfitLossInput input);

    Task<ListResultDto<ShopProfitLossTrendPointDto>> GetTrendAsync(GetShopProfitLossInput input);

    Task<ListResultDto<ShopProfitLossExpenseCategoryDto>> GetExpenseBreakdownAsync(GetShopProfitLossInput input);

    Task<ListResultDto<ShopProfitLossProductContributionDto>> GetProductContributionAsync(GetShopProfitLossInput input);

    Task<ShopProfitLossComparisonDto> GetComparisonAsync(GetShopProfitLossInput input);

    Task<IRemoteStreamContent> ExportAsync(GetShopProfitLossInput input, ShopReportExportFormat format);
}
