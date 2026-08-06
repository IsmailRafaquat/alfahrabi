using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.PrintSettings;

public interface IShopPrintSettingsAppService : IApplicationService
{
    Task<ShopPrintSettingsDto> GetAsync();
    Task<ShopPrintSettingsDto> CreateOrUpdateAsync(CreateUpdateShopPrintSettingsDto input);
}
