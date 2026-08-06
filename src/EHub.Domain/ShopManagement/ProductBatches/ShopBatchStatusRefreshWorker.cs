using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Threading;
using Volo.Abp.Uow;

namespace EHub.ShopManagement.ProductBatches;

/// <summary>
/// Runs once a day and recomputes Active/NearExpiry/Expired/Exhausted for every tenant's batches -
/// nothing else (no notifications). Iterates tenants explicitly because ShopProductBatch is
/// tenant-isolated via a query filter keyed on ICurrentTenant, which has no ambient value here.
/// </summary>
public class ShopBatchStatusRefreshWorker : AsyncPeriodicBackgroundWorkerBase
{
    public ShopBatchStatusRefreshWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
        : base(timer, serviceScopeFactory)
    {
        Timer.Period = 24 * 60 * 60 * 1000;
    }

    protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
    {
        var tenantRepository = workerContext.ServiceProvider.GetRequiredService<ITenantRepository>();
        var currentTenant = workerContext.ServiceProvider.GetRequiredService<ICurrentTenant>();
        var unitOfWorkManager = workerContext.ServiceProvider.GetRequiredService<IUnitOfWorkManager>();
        var batchManager = workerContext.ServiceProvider.GetRequiredService<ShopProductBatchManager>();

        var tenants = await tenantRepository.GetListAsync();

        foreach (var tenant in tenants)
        {
            using var change = currentTenant.Change(tenant.Id);
            using var uow = unitOfWorkManager.Begin(requiresNew: true);
            await batchManager.RefreshAllStatusesAsync();
            await uow.CompleteAsync();
        }
    }
}
