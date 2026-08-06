using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.Products;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Notifications.Generators;

/// <summary>Low Stock (0 &lt; CurrentStock &lt;= ReorderLevel) and Out of Stock (CurrentStock &lt;= 0).</summary>
public class LowStockNotificationGenerator : IShopNotificationGenerator, ITransientDependency
{
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly ShopNotificationManager _notificationManager;
    private readonly ShopNotificationSettingsProvider _settingsProvider;

    public LowStockNotificationGenerator(
        IRepository<ShopProduct, Guid> productRepository,
        ShopNotificationManager notificationManager,
        ShopNotificationSettingsProvider settingsProvider)
    {
        _productRepository = productRepository;
        _notificationManager = notificationManager;
        _settingsProvider = settingsProvider;
    }

    public async Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled) return;

        var products = await _productRepository.GetListAsync(x => x.TenantId == tenantId && x.IsActive);

        var lowStockActive = new HashSet<Guid>();
        var outOfStockActive = new HashSet<Guid>();

        foreach (var product in products)
        {
            if (settings.OutOfStockNotificationsEnabled && product.CurrentStock <= 0)
            {
                outOfStockActive.Add(product.Id);
                await EvaluateOutOfStockAsync(tenantId, product);
            }
            else if (settings.LowStockNotificationsEnabled && product.ReorderLevel > 0 && product.CurrentStock > 0 && product.CurrentStock <= product.ReorderLevel)
            {
                lowStockActive.Add(product.Id);
                await EvaluateLowStockAsync(tenantId, product);
            }
        }

        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.LowStock, lowStockActive);
        await _notificationManager.ResolveStaleAsync(tenantId, ShopNotificationType.OutOfStock, outOfStockActive);
    }

    /// <summary>Narrow re-check for one product, used by immediate hooks after a stock-affecting transaction.</summary>
    public async Task EvaluateProductAsync(Guid tenantId, Guid productId)
    {
        var product = await _productRepository.FindAsync(productId);
        if (product == null || product.TenantId != tenantId) return;

        var settings = await _settingsProvider.GetOrDefaultAsync(tenantId);
        if (!settings.NotificationsEnabled) return;

        if (settings.OutOfStockNotificationsEnabled && product.CurrentStock <= 0)
        {
            await EvaluateOutOfStockAsync(tenantId, product);
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, LowStockKey(tenantId, product.Id));
        }
        else if (settings.LowStockNotificationsEnabled && product.ReorderLevel > 0 && product.CurrentStock > 0 && product.CurrentStock <= product.ReorderLevel)
        {
            await EvaluateLowStockAsync(tenantId, product);
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, OutOfStockKey(tenantId, product.Id));
        }
        else
        {
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, LowStockKey(tenantId, product.Id));
            await _notificationManager.ResolveBySourceKeyAsync(tenantId, OutOfStockKey(tenantId, product.Id));
        }
    }

    private Task EvaluateLowStockAsync(Guid tenantId, ShopProduct product) =>
        _notificationManager.CreateOrUpdateAsync(
            tenantId, ShopNotificationType.LowStock, ShopNotificationSeverity.Warning,
            "Low Stock",
            $"Product \"{product.Name}\" is low in stock. Current stock: {product.CurrentStock}, Reorder level: {product.ReorderLevel}.",
            LowStockKey(tenantId, product.Id),
            referenceType: "Product", referenceId: product.Id, referenceNumber: product.Code,
            navigationUrl: ShopNotificationNavigation.Product(product.Id), actionLabel: "ViewProduct");

    private Task EvaluateOutOfStockAsync(Guid tenantId, ShopProduct product) =>
        _notificationManager.CreateOrUpdateAsync(
            tenantId, ShopNotificationType.OutOfStock, ShopNotificationSeverity.Critical,
            "Out of Stock",
            $"Product \"{product.Name}\" is out of stock.",
            OutOfStockKey(tenantId, product.Id),
            referenceType: "Product", referenceId: product.Id, referenceNumber: product.Code,
            navigationUrl: ShopNotificationNavigation.Product(product.Id), actionLabel: "ViewProduct");

    private static string LowStockKey(Guid tenantId, Guid productId) => $"LowStock:{tenantId}:{productId}";
    private static string OutOfStockKey(Guid tenantId, Guid productId) => $"OutOfStock:{tenantId}:{productId}";
}
