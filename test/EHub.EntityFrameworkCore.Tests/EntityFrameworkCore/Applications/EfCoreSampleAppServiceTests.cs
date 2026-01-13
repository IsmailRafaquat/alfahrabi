using EHub.Samples;
using Xunit;

namespace EHub.EntityFrameworkCore.Applications;

[Collection(EHubTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<EHubEntityFrameworkCoreTestModule>
{

}
