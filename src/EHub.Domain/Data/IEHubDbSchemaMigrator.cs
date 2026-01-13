using System.Threading.Tasks;

namespace EHub.Data;

public interface IEHubDbSchemaMigrator
{
    Task MigrateAsync();
}
