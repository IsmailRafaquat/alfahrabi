using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Notifications;

public interface IShopNotificationAppService : IApplicationService
{
    Task<PagedResultDto<ShopNotificationDto>> GetListAsync(GetShopNotificationsInput input);

    Task<ShopNotificationDto> GetAsync(Guid id);

    Task<ShopNotificationSummaryDto> GetSummaryAsync();

    Task MarkAsReadAsync(Guid id);

    Task MarkAsUnreadAsync(Guid id);

    Task MarkAllAsReadAsync();

    Task DismissAsync(Guid id);

    Task DismissAllAsync();

    Task DeleteAsync(Guid id);

    Task<ShopNotificationSettingsDto> GetSettingsAsync();

    Task UpdateSettingsAsync(UpdateShopNotificationSettingsDto input);

    Task GenerateNowAsync();
}
