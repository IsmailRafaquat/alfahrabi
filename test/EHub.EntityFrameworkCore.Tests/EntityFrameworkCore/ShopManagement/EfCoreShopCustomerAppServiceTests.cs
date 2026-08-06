using EHub.ShopManagement.Customers;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopCustomerAppServiceTests : ShopCustomerAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
