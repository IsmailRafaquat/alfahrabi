using EHub.ShopManagement.SaleReturns;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopSaleReturnAppServiceTests : ShopSaleReturnAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
