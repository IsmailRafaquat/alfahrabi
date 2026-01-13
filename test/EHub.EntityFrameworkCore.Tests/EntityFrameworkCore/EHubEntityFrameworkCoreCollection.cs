using Xunit;

namespace EHub.EntityFrameworkCore;

[CollectionDefinition(EHubTestConsts.CollectionDefinitionName)]
public class EHubEntityFrameworkCoreCollection : ICollectionFixture<EHubEntityFrameworkCoreFixture>
{

}
