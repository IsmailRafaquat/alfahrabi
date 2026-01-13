using EHub.Localization;
using Volo.Abp.Application.Services;

namespace EHub;

/* Inherit your application services from this class.
 */
public abstract class EHubAppService : ApplicationService
{
    protected EHubAppService()
    {
        LocalizationResource = typeof(EHubResource);
    }
}
