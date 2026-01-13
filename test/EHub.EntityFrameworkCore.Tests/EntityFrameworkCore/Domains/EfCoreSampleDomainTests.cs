using EHub.Samples;
using Xunit;

namespace EHub.EntityFrameworkCore.Domains;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<EHubEntityFrameworkCoreTestModule>
{

}
