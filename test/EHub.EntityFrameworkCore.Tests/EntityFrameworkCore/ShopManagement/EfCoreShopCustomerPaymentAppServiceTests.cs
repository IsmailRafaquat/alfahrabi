using EHub.ShopManagement.CustomerPayments;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopCustomerPaymentAppServiceTests : ShopCustomerPaymentAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
}
