using EHub.ShopManagement.SupplierPayments;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopSupplierPaymentAppServiceTests : ShopSupplierPaymentAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
