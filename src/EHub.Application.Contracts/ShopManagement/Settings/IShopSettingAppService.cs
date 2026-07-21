using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Settings;

public interface IShopSettingAppService : IApplicationService
{
    Task<ShopSettingDto> GetAsync();
    Task<ShopSettingDto> CreateOrUpdateAsync(CreateUpdateShopSettingDto input);
    Task<ShopSettingSetupStatusDto> GetSetupStatusAsync();
}
