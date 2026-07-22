using EHub.ShopManagement.GoodsReceipts;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopGoodsReceiptAppServiceTests : ShopGoodsReceiptAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
