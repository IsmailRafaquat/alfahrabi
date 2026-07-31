using System;
using System.Linq;
using System.Net.Http;
using EHub.ShopManagement.AiAssistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.Account;
using Volo.Abp.Identity;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;

namespace EHub;

[DependsOn(
    typeof(EHubDomainModule),
    typeof(EHubApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class EHubApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<EHubApplicationModule>();
        });

        var configuration = context.Services.GetConfiguration();
        Configure<ShopAiOptions>(configuration.GetSection("ShopAi"));
        Configure<ShopAiSpeechOptions>(configuration.GetSection("ShopAiSpeech"));

        ConfigureShopAiHttpClients(context);
    }

    // TEMPORARY diagnostic - remove once the intent-misclassification investigation is done.
    // Checked at OnApplicationInitialization (after ALL modules' ConfigureServices AND ABP's
    // conventional assembly-scan registration have completed) so this is the most authoritative
    // point to confirm whether IShopAiActionHandler implementations actually made it into the
    // container, independent of anything ShopAiActionHandlerRegistry itself does.
    public override void OnApplicationInitialization(Volo.Abp.ApplicationInitializationContext context)
    {
        var handlers = context.ServiceProvider.GetServices<IShopAiActionHandler>().ToList();
        var logger = context.ServiceProvider.GetRequiredService<ILogger<EHubApplicationModule>>();
        logger.LogInformation(
            "STARTUP CHECK: resolved {Count} IShopAiActionHandler via OnApplicationInitialization: {Types}",
            handlers.Count, string.Join(",", handlers.Select(h => h.GetType().FullName)));
    }

    /// <summary>
    /// Both clients are strictly outbound HTTP to local infrastructure (Ollama, faster-whisper) -
    /// neither is ever given a connection string, a repository, or a DbContext.
    /// </summary>
    private static void ConfigureShopAiHttpClients(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient<IShopOllamaClient, ShopOllamaClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ShopAiOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.RequestTimeoutSeconds) + 10);
        });

        context.Services.AddHttpClient<IShopSpeechToTextClient, ShopSpeechToTextClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ShopAiSpeechOptions>>().Value;
            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
            {
                client.BaseAddress = baseUri;
            }
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, options.RequestTimeoutSeconds) + 10);
        });
    }
}
