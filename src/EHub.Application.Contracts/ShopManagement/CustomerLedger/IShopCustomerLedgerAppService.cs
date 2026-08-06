using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.CustomerLedger;

public interface IShopCustomerLedgerAppService : IApplicationService
{
    Task<ShopCustomerLedgerDto> GetLedgerAsync(GetShopCustomerLedgerInput input);
    Task<ShopCustomerBalanceSummaryDto> GetBalanceSummaryAsync(Guid customerId);
    Task<ShopCustomerStatementDto> GetStatementAsync(GetShopCustomerLedgerInput input);
}
