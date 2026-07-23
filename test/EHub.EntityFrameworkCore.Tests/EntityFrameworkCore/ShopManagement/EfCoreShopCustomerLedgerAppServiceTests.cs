using System.Linq;
using EHub.ShopManagement.CustomerLedger;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace EHub.EntityFrameworkCore.ShopManagement;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreShopCustomerLedgerAppServiceTests : ShopCustomerLedgerAppServiceTests<EHubEntityFrameworkCoreTestModule>
{
    [Fact]
    public void No_New_Customer_Ledger_Transaction_Table_Is_Created()
    {
        var dbSetProperties = typeof(EHubDbContext).GetProperties()
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.Name)
            .ToList();

        dbSetProperties.ShouldNotContain(x => x.Contains("CustomerLedger"));
    }
}
