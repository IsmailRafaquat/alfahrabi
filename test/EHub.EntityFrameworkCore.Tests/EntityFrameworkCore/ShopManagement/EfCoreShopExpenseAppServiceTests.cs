using EHub.ShopManagement.Expenses;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopExpenseAppServiceTests : ShopExpenseAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
