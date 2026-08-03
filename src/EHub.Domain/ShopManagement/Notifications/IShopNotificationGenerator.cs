using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHub.ShopManagement.Notifications;

/// <summary>One rule-check per implementation - a full-tenant sweep run by the background job (and, for some, an on-demand narrow check run from an immediate hook).</summary>
public interface IShopNotificationGenerator
{
    Task GenerateAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
