using EHub.ShopManagement.Suppliers;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopSupplierAppServiceTests : ShopSupplierAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
