using EHub.ShopManagement.PurchaseOrders;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopPurchaseOrderAppServiceTests : ShopPurchaseOrderAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
