using Volo.Abp.Modularity;

namespace EHub;

[DependsOn(
    typeof(EHubApplicationModule),
    typeof(EHubDomainTestModule)
)]
public class EHubApplicationTestModule : AbpModule
{

}
