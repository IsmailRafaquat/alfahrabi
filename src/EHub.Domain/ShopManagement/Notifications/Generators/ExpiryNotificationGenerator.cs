using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.Products;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>Near Expiry (expires within the product/tenant alert window, still has stock) and Expired Batch (already past expiry, still has stock).</summary>
public class ExpiryNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopProductBatch, Guid> _batchRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;
    private readonly IClock _clock;

    public ExpiryNotificationGenerator(
        IRepository<ShopProductBatch, Guid> batchRepository,
        IRepository<ShopProduct, Guid> productRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider,
        IClock clock)
    {
        _batchRepository = batchRepository;
        _productRepository = productRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
        _clock = clock;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled) return;
        if (!settings.NearExpiryNotificationsEnabled && !settings.ExpiredBatchNotificationsEnabled) return;

        var today = _clock.Now.Date;

        var batches = await _batchRepository.GetListAsync(x => x.TenantId == tenantId && x.AvailableQuantity > 0 && x.ExpiryDate.HasValue);
        if (batches.Count == 0) return;

        var productIds = batches.Select(x => x.ProductId).Distinct().ToList();
        var products = await _productRepository.GetListAsync(x => productIds.Contains(x.Id));
        var alertDaysByProduct = products.ToDictionary(x => x.Id, x => x.ExpiryAlertDays ?? settings.NearExpiryDefaultDays);
        var nameByProduct = products.ToDictionary(x => x.Id, x => x.Name);

        var nearExpiryActive = new HashSet<Guid>();
        var expiredActive = new HashSet<Guid>();

        foreach (var batch in batches)
        {
            var expiryDate = batch.ExpiryDate!.Value.Date;
            var productName = nameByProduct.GetValueOrDefault(batch.ProductId, "Unknown product");

            if (expiryDate < today)
            {
                if (!settings.ExpiredBatchNotificationsEnabled) continue;
                expiredActive.Add(batch.Id);
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.ExpiredBatch, ShopNotificationSeverity.Critical,
                    "Expired Batch",
                    $"Batch \"{batch.BatchNumber}\" for product \"{productName}\" has expired.",
                    $"ExpiredBatch:{tenantId}:{batch.Id}",
                    referenceType: "ProductBatch", referenceId: batch.Id, referenceNumber: batch.BatchNumber,
                    navigationUrl: ShopNotificationNavigation.ProductBatch(batch.ProductId), actionLabel: "ViewBatch");
            }
            else
            {
                if (!settings.NearExpiryNotificationsEnabled) continue;
                var alertDays = alertDaysByProduct.GetValueOrDefault(batch.ProductId, settings.NearExpiryDefaultDays);
                if ((expiryDate - today).Days > alertDays) continue;

                var daysRemaining = (expiryDate - today).Days;
                nearExpiryActive.Add(batch.Id);
                await _notificationManager.CreateOrUpdateAsync(
                    tenantId, ShopNotificationType.NearExpiry, ShopNotificationSeverity.Warning,
                    "Near Expiry",
                    $"Batch \"{batch.BatchNumber}\" for product \"{productName}\" will expire in {daysRemaining} days.",
                    $"NearExpiry:{tenantId}:{batch.Id}:{expiryDate:yyyyMMdd}",
                    referenceType: "ProductBatch", referenceId: batch.Id, referenceNumber: batch.BatchNumber,
                    navigationUrl: ShopNotificationNavigation.ProductBatch(batch.ProductId), actionLabel: "ViewBatch");
            }
        }

        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.NearExpiry, nearExpiryActive);
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.ExpiredBatch, expiredActive);
    }
}
