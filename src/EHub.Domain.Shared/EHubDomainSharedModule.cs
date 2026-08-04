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
            options.Languages.Add(new LanguageInfo("en-GB", "en-GB", "English (United Kingdom)")); 
            options.Languages.Add(new LanguageInfo("zh-Hans", "zh-Hans", "简体中文")); 
            options.Languages.Add(new LanguageInfo("es", "es", "Español")); 
            options.Languages.Add(new LanguageInfo("ar", "ar", "العربية")); 
            options.Languages.Add(new LanguageInfo("hi", "hi", "हिन्दी")); 
            options.Languages.Add(new LanguageInfo("pt-BR", "pt-BR", "Português (Brasil)")); 
            options.Languages.Add(new LanguageInfo("fr", "fr", "Français")); 
            options.Languages.Add(new LanguageInfo("ru", "ru", "Русский")); 
            options.Languages.Add(new LanguageInfo("de-DE", "de-DE", "Deutsch (Deutschland)")); 
            options.Languages.Add(new LanguageInfo("tr", "tr", "Türkçe")); 
            options.Languages.Add(new LanguageInfo("it", "it", "Italiano")); 
            options.Languages.Add(new LanguageInfo("cs", "cs", "Čeština")); 
            options.Languages.Add(new LanguageInfo("hu", "hu", "Magyar")); 
            options.Languages.Add(new LanguageInfo("ro-RO", "ro-RO", "Română (România)")); 
            options.Languages.Add(new LanguageInfo("sv", "sv", "Svenska")); 
            options.Languages.Add(new LanguageInfo("fi", "fi", "Suomi")); 
            options.Languages.Add(new LanguageInfo("sk", "sk", "Slovenčina")); 
            options.Languages.Add(new LanguageInfo("is", "is", "Íslenska")); 
            options.Languages.Add(new LanguageInfo("zh-Hant", "zh-Hant", "繁體中文"));
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
        // AbpUiNavigationResource is registered by a UI/theme module that runs after this module's
        // ConfigureServices, so it isn't available yet there. PostConfigureServices runs after every
        // module's ConfigureServices has completed, so it's guaranteed to exist by this point.
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources.Get<AbpUiNavigationResource>().AddVirtualJson("/Localization/AbpUiNavigation");
            options.Resources.Get<AbpUiResource>().AddVirtualJson("/Localization/AbpUi");
        });
    }
}
