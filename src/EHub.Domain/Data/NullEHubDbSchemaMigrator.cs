using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EHub.Data;

/* This is used if database provider does't define
 * IEHubDbSchemaMigrator implementation.
 */
public class NullEHubDbSchemaMigrator : IEHubDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
