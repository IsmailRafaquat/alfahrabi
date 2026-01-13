using Volo.Abp.Modularity;

namespace EHub;

public abstract class EHubApplicationTestBase<TStartupModule> : EHubTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
