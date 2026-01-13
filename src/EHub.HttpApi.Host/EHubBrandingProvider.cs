using Microsoft.Extensions.Localization;
using EHub.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace EHub;

[Dependency(ReplaceServices = true)]
public class EHubBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EHubResource> _localizer;

    public EHubBrandingProvider(IStringLocalizer<EHubResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
