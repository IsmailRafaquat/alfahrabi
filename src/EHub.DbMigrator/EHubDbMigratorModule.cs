using EHub.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EHub.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EHubEntityFrameworkCoreModule),
    typeof(EHubApplicationContractsModule)
)]
public class EHubDbMigratorModule : AbpModule
{
}
