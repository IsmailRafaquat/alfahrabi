using EHub.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.SettingManagement.Localization;
using Volo.Abp.UI.Navigation.Localization.Resource;
using Localization.Resources.AbpUi;
using Volo.Abp.VirtualFileSystem;
using Volo.Abp.OpenIddict;
using Volo.Abp.BlobStoring.Database;
using Volo.Abp.TenantManagement;

namespace EHub;

[DependsOn(
    typeof(AbpAuditLoggingDomainSharedModule),
    typeof(AbpBackgroundJobsDomainSharedModule),
    typeof(AbpFeatureManagementDomainSharedModule),
    typeof(AbpPermissionManagementDomainSharedModule),
    typeof(AbpSettingManagementDomainSharedModule),
    typeof(AbpIdentityDomainSharedModule),
    typeof(AbpOpenIddictDomainSharedModule),
    typeof(AbpTenantManagementDomainSharedModule),
    typeof(BlobStoringDatabaseDomainSharedModule)
    )]
public class EHubDomainSharedModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        EHubGlobalFeatureConfigurator.Configure();
        EHubModuleExtensionConfigurator.Configure();
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<EHubDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<EHubResource>("en")
                .AddBaseTypes(typeof(AbpValidationResource))
                .AddVirtualJson("/Localization/EHub");

            options.Resources.Get<IdentityResource>().AddVirtualJson("/Localization/AbpIdentity");
            options.Resources.Get<AbpSettingManagementResource>().AddVirtualJson("/Localization/AbpSettingManagement");

            options.DefaultResourceType = typeof(EHubResource);
            
            options.Languages.Add(new LanguageInfo("en", "en", "English"));
            options.Languages.Add(new LanguageInfo("ur", "ur", "اردو"));

        });
        
        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("EHub", typeof(EHubResource));
            options.MapCodeNamespace("ShopManagement", typeof(EHubResource));
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        // AbpUiNavigationResource/AbpUiResource are registered by a UI/theme module (the MVC/Angular
        // theme packages EHub.HttpApi.Host depends on) that runs after this module's ConfigureServices,
        // so they aren't available yet there - PostConfigureServices runs after every module's
        // ConfigureServices has completed, so they're guaranteed to exist by this point *for hosts that
        // reference a UI module*. Entry points with no UI at all, like EHub.DbMigrator, never register
        // these types, so the lookup is guarded here instead of assuming every host has them.
        Configure<AbpLocalizationOptions>(options =>
        {
            if (options.Resources.ContainsResource(typeof(AbpUiNavigationResource)))
            {
                options.Resources.Get<AbpUiNavigationResource>().AddVirtualJson("/Localization/AbpUiNavigation");
            }

            if (options.Resources.ContainsResource(typeof(AbpUiResource)))
            {
                options.Resources.Get<AbpUiResource>().AddVirtualJson("/Localization/AbpUi");
            }
        });
    }
}
