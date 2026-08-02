using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using EHub.EntityFrameworkCore;
using EHub.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.Autofac;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;

namespace EHub.McpServer;

/// <summary>
/// A separate ABP host, deliberately NOT part of EHubHttpApiHostModule's process - it exists so the
/// MCP endpoint can be deployed, scaled, and (per "do not expose the MCP endpoint publicly without
/// authentication") firewalled independently from the main API. It reuses the SAME OpenIddict issuer
/// as the main host (same database, same signing keys, same "EHub" audience) via the identical
/// AddValidation/UseLocalServer wiring EHubHttpApiHostModule already uses - a token obtained by
/// logging into the main Angular app is valid here without any separate login or token exchange.
/// </summary>
[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpAspNetCoreMultiTenancyModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(EHubApplicationModule),
    typeof(EHubEntityFrameworkCoreModule),
    typeof(AbpAccountWebOpenIddictModule)
    )]
public class EHubMcpServerModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var hostingEnvironment = context.Services.GetHostingEnvironment();

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("EHub");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.SetIssuer(new Uri(configuration["AuthServer:Authority"]!));
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();

        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });

        context.Services.AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly(typeof(EHubMcpServerModule).Assembly);

        ConfigureCors(context, configuration);
    }

    private static void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithOrigins(
                        configuration["App:CorsOrigins"]?
                            .Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(o => o.Trim().RemovePostFix("/"))
                            .ToArray() ?? Array.Empty<string>())
                    .WithAbpExposedHeaders()
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseCors();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            // Every tool independently re-checks permissions via IShopAiCommandValidator/
            // AuthorizationService (same as the existing internal handlers) - RequireAuthorization()
            // here only rejects anonymous callers before a request ever reaches a tool, it does not
            // replace per-tool permission checks.
            endpoints.MapMcp("/mcp").RequireAuthorization();
        });
    }
}
