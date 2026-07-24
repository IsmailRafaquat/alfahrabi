using EHub.ShopManagement.CashRegisters;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopCashRegisterAppServiceTests : ShopCashRegisterAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
