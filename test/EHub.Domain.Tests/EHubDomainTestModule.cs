using Volo.Abp.Modularity;

namespace EHub;

[DependsOn(
    typeof(EHubDomainModule),
    typeof(EHubTestBaseModule)
)]
public class EHubDomainTestModule : AbpModule
{

}
