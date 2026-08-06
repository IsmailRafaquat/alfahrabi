using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Notifications;

/// <summary>Reads a tenant's notification settings, falling back to an unsaved default instance so generators work before a tenant has ever opened the settings page.</summary>
public class ShopNotificationSettingsProvider : ITransientDependency
{
    private readonly IRepository<ShopNotificationSettings, Guid> _repository;

    public ShopNotificationSettingsProvider(IRepository<ShopNotificationSettings, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<ShopNotificationSettings> GetOrDefaultAsync(Guid tenantId)
    {
        var existing = await _repository.FirstOrDefaultAsync(x => x.TenantId == tenantId);
        return existing ?? new ShopNotificationSettings(Guid.Empty, tenantId);
    }
}
