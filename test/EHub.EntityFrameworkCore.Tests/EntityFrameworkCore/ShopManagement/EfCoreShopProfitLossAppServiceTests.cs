using EHub.ShopManagement.ProfitLoss;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopProfitLossAppServiceTests : ShopProfitLossAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
