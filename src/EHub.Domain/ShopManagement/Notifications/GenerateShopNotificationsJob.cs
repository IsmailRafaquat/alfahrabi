using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace EHub.ShopManagement.Notifications;

/// <summary>
/// Runs every 30 minutes: for each active tenant, runs every registered
/// <see cref="IShopNotificationGenerator"/> in sequence, then deletes expired/old notifications per
/// that tenant's retention setting. Wrapped per-tenant in try/catch so one tenant's failure never
/// blocks the rest. <see cref="Volo.Abp.BackgroundWorkers.AsyncPeriodicBackgroundWorkerBase"/> awaits
/// DoWorkAsync to completion before rescheduling, so runs never overlap.
/// </summary>
public class GenerateShopNotificationsJob : AsyncPeriodicBackgroundWorkerBase
{
    public GenerateShopNotificationsJob(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 30 * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var tenantRepository = workerContext.ServiceProvider.GetRequiredService<ITenantRepository>();
        var currentTenant = workerContext.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var logger = workerContext.ServiceProvider.GetRequiredService<ILogger<GenerateShopNotificationsJob>>();
        var notificationManager = workerContext.ServiceProvider.GetRequiredService<ShopNotificationManager>();
        var settingsProvider = workerContext.ServiceProvider.GetRequiredService<ShopNotificationSettingsProvider>();

        var tenants = await tenantRepository.GetListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                using var change = currentTenant.Change(tenant.Id);
                using var uow = unitOfWorkManager.Begin(requiresNew: true);

                // Resolved per-tenant scope so any tenant-scoped dependencies inside a generator see the right ICurrentTenant.
                var generators = workerContext.ServiceProvider.GetServices<IShopNotificationGenerator>();
                foreach (var generator in generators)
                {
                    await generator.GenerateAsync(tenant.Id);
                }

                var settings = await settingsProvider.GetOrDefaultAsync(tenant.Id);
                await notificationManager.DeleteExpiredNotificationsAsync(tenant.Id, settings.NotificationRetentionDays);

                await uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Shop notification generation failed for tenant {TenantId}", tenant.Id);
            }
        }
    }
}
