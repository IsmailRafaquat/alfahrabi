using System;
using System.Linq;
using System.Net.Http;
using EHub.ShopManagement.AiAssistant;
using EHub.ShopManagement.AiAssistant.Handlers;
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
        Configure<ShopMcpOptions>(configuration.GetSection("ShopMcp"));

        ConfigureShopAiHttpClients(context);

        // ABP's conventional registrar (ITransientDependency scan) was not picking these three up
        // for reasons not yet root-caused - GetTodaySalesAiHandler, CreateCustomerAiHandler, and
        // CreateUnitAiHandler all showed 0 resolutions via IEnumerable<IShopAiActionHandler> even
        // at OnApplicationInitialization, despite identical declarations to sibling classes in the
        // same assembly that DO get registered conventionally. Explicit registration here is a
        // known-correct fix regardless of the underlying cause; ShopAiActionHandlerRegistry also
        // now de-duplicates by ActionType so this is safe even if the convention scan starts
        // registering these too after some future ABP/tooling update.
        context.Services.AddTransient<IShopAiActionHandler, GetTodaySalesAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, CreateCustomerAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, CreateUnitAiHandler>();

        // Same defensive explicit registration as the three handlers above - registered here rather
        // than relying solely on the conventional ITransientDependency scan.
        context.Services.AddTransient<IShopAiActionHandler, GetUnitsAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetProductCategoriesAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetProductsAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetCustomersAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetSuppliersAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetExpenseCategoriesAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetBankAccountsAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetSalesAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetPurchaseOrdersAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetExpensesAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetStockAdjustmentsAiHandler>();
        context.Services.AddTransient<IShopAiActionHandler, GetPhysicalStockCountsAiHandler>();
    }

    // TEMPORARY diagnostic - remove once the intent-misclassification investigation is done.
    // Checked at OnApplicationInitialization (after ALL modules' ConfigureServices AND ABP's
    // conventional assembly-scan registration have completed) so this is the most authoritative
    // point to confirm whether IShopAiActionHandler implementations actually made it into the
    // container, independent of anything ShopAiActionHandlerRegistry itself does.
    public override void OnApplicationInitialization(Volo.Abp.ApplicationInitializationContext context)
    {
        // Some IShopAiActionHandler implementations (and their dependency chains, e.g.
        // AbpAuthorizationService) are scoped services, so they must be resolved from a
        // created scope rather than the root service provider.
        using var scope = context.ServiceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IShopAiActionHandler>().ToList();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<EHubApplicationModule>>();
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
