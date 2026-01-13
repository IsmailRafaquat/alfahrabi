using EHub.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EHub.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class EHubController : AbpControllerBase
{
    protected EHubController()
    {
        LocalizationResource = typeof(EHubResource);
    }
}
