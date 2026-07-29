using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public interface IShopReportAppService : IApplicationService
{
    Task<ShopSalesReportResultDto> GetSalesReportAsync(GetShopSalesReportInput input);
    Task<IRemoteStreamContent> ExportSalesReportAsync(GetShopSalesReportInput input, ShopReportExportFormat format);

    Task<ShopPurchaseReportResultDto> GetPurchaseReportAsync(GetShopPurchaseReportInput input);
    Task<IRemoteStreamContent> ExportPurchaseReportAsync(GetShopPurchaseReportInput input, ShopReportExportFormat format);

    Task<ShopStockReportResultDto> GetStockReportAsync(GetShopStockReportInput input);
    Task<IRemoteStreamContent> ExportStockReportAsync(GetShopStockReportInput input, ShopReportExportFormat format);

    Task<ShopStockMovementReportResultDto> GetStockMovementReportAsync(GetShopStockMovementReportInput input);
    Task<IRemoteStreamContent> ExportStockMovementReportAsync(GetShopStockMovementReportInput input, ShopReportExportFormat format);

    Task<ShopBatchExpiryReportResultDto> GetBatchExpiryReportAsync(GetShopBatchExpiryReportInput input);
    Task<IRemoteStreamContent> ExportBatchExpiryReportAsync(GetShopBatchExpiryReportInput input, ShopReportExportFormat format);

    Task<ShopCustomerReceivableReportResultDto> GetCustomerReceivablesReportAsync(GetShopCustomerReceivablesReportInput input);
    Task<IRemoteStreamContent> ExportCustomerReceivablesReportAsync(GetShopCustomerReceivablesReportInput input, ShopReportExportFormat format);

    Task<ShopCustomerTransactionReportResultDto> GetCustomerTransactionReportAsync(GetShopCustomerTransactionReportInput input);

    Task<ShopSupplierPayableReportResultDto> GetSupplierPayablesReportAsync(GetShopSupplierPayablesReportInput input);
    Task<IRemoteStreamContent> ExportSupplierPayablesReportAsync(GetShopSupplierPayablesReportInput input, ShopReportExportFormat format);

    Task<ShopSupplierTransactionReportResultDto> GetSupplierTransactionReportAsync(GetShopSupplierTransactionReportInput input);

    Task<ShopExpenseReportResultDto> GetExpenseReportAsync(GetShopExpenseReportInput input);
    Task<IRemoteStreamContent> ExportExpenseReportAsync(GetShopExpenseReportInput input, ShopReportExportFormat format);

    Task<ShopCashReportResultDto> GetCashReportAsync(GetShopCashReportInput input);
    Task<IRemoteStreamContent> ExportCashReportAsync(GetShopCashReportInput input, ShopReportExportFormat format);

    Task<ShopBankTransactionReportResultDto> GetBankTransactionReportAsync(GetShopBankTransactionReportInput input);
    Task<IRemoteStreamContent> ExportBankTransactionReportAsync(GetShopBankTransactionReportInput input, ShopReportExportFormat format);

    Task<ShopTaxSummaryReportDto> GetTaxSummaryReportAsync(GetShopTaxSummaryReportInput input);

    Task<ShopProductPerformanceReportResultDto> GetProductPerformanceReportAsync(GetShopProductPerformanceReportInput input);
    Task<IRemoteStreamContent> ExportProductPerformanceReportAsync(GetShopProductPerformanceReportInput input, ShopReportExportFormat format);
}
