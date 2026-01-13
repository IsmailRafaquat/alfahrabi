using Volo.Abp.Modularity;

namespace EHub;

/* Inherit from this class for your domain layer tests. */
public abstract class EHubDomainTestBase<TStartupModule> : EHubTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
