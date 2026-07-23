using EHub.ShopManagement.Sales;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopSaleAppServiceTests : ShopSaleAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
