using EHub.ShopManagement.Products;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopProductAppServiceTests : ShopProductAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
