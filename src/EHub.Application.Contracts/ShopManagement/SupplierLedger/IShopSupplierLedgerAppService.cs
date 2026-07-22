using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.SupplierLedger;

public interface IShopSupplierLedgerAppService : IApplicationService
{
    Task<ShopSupplierLedgerDto> GetLedgerAsync(GetShopSupplierLedgerInput input);
    Task<ShopSupplierBalanceSummaryDto> GetBalanceSummaryAsync(Guid supplierId);
    Task<ShopSupplierStatementDto> GetStatementAsync(GetShopSupplierLedgerInput input);
}
