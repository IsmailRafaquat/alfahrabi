using EHub.ShopManagement.SupplierLedger;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopSupplierLedgerAppServiceTests : ShopSupplierLedgerAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
