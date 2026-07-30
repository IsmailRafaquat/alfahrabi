using System;
using System.Net.Http;
using EHub.ShopManagement.AiAssistant;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
