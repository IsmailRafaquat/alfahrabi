using EHub.ShopManagement.ExpenseCategories;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopExpenseCategoryAppServiceTests : ShopExpenseCategoryAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
