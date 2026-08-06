using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintSettings;
using EHub.ShopManagement.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.PrintTemplates;

// Shared "business letterhead" resolution used by every per-document print mapper - reads
// ShopSetting (shop name/address/phone/etc.) and ShopPrintSetting (which of those to show, plus
// the other print toggles) for the CURRENT tenant once, so mappers never each re-implement this
// lookup. This is cross-cutting infrastructure, not document-specific mapping logic, so it stays
// out of the "each module owns its own mapper" rule the per-document mappers otherwise follow.
public class ShopPrintContext
{
    public ShopSetting? ShopSetting { get; set; }
    public ShopPrintSetting? PrintSetting { get; set; }
}

public interface IShopPrintContextProvider
{
    Task<ShopPrintContext> GetContextAsync();
    ShopPrintBusinessDto BuildBusinessDto(ShopPrintContext context);
}

public class ShopPrintContextProvider : IShopPrintContextProvider, ITransientDependency
{
    private readonly IRepository<ShopSetting, Guid> _shopSettingRepository;
    private readonly IRepository<ShopPrintSetting, Guid> _printSettingRepository;
    private readonly ICurrentTenant _currentTenant;

    public ShopPrintContextProvider(
        IRepository<ShopSetting, Guid> shopSettingRepository,
        IRepository<ShopPrintSetting, Guid> printSettingRepository,
        ICurrentTenant currentTenant)
    {
        _shopSettingRepository = shopSettingRepository;
        _printSettingRepository = printSettingRepository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopPrintContext> GetContextAsync()
    {
        var tenantId = _currentTenant.Id;
        if (!tenantId.HasValue)
            return new ShopPrintContext();

        var shopSettingQuery = await _shopSettingRepository.GetQueryableAsync();
        var shopSetting = shopSettingQuery.FirstOrDefault(x => x.TenantId == tenantId.Value);

        var printSettingQuery = await _printSettingRepository.GetQueryableAsync();
        var printSetting = printSettingQuery.FirstOrDefault(x => x.TenantId == tenantId.Value);

        return new ShopPrintContext { ShopSetting = shopSetting, PrintSetting = printSetting };
    }

    public ShopPrintBusinessDto BuildBusinessDto(ShopPrintContext context)
    {
        var shop = context.ShopSetting;
        var print = context.PrintSetting;

        var addressParts = new[] { shop?.AddressLine1, shop?.AddressLine2, shop?.City }
            .Where(x => !string.IsNullOrWhiteSpace(x));

        return new ShopPrintBusinessDto
        {
            ShopName = shop?.ShopDisplayName ?? string.Empty,
            AddressLine = addressParts.Any() ? string.Join(", ", addressParts) : null,
            Phone = shop?.Phone,
            Email = shop?.Email,
            TaxNumber = shop?.TaxNumber,
            CurrencyCode = shop?.CurrencyCode ?? "PKR",
            CurrencySymbol = shop?.CurrencySymbol ?? "₨",
            DecimalPlaces = shop?.DecimalPlaces ?? 2,

            // PrintHeaderLogo ships as a toggle only - no logo upload/serve endpoint exists yet
            // anywhere in this codebase, so this is always false regardless of the setting until
            // that (separate, out-of-scope-for-printing) feature is built.
            ShowLogo = false,
            ShowShopName = print?.PrintShopName ?? true,
            ShowAddress = print?.PrintShopAddress ?? true,
            ShowPhone = print?.PrintShopPhone ?? true,
            ShowEmail = print?.PrintShopEmail ?? false,
            ShowTaxNumber = print?.PrintTaxNumber ?? false
        };
    }
}
