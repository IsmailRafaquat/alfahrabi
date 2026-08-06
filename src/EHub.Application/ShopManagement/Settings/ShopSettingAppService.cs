using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Settings;

[Authorize]
public class ShopSettingAppService : ApplicationService, IShopSettingAppService
{
    private readonly IRepository<ShopSetting, Guid> _repository;

    public ShopSettingAppService(IRepository<ShopSetting, Guid> repository) => _repository = repository;

    [Authorize(EHubPermissions.ShopSettings.Default)]
    public async Task<ShopSettingDto> GetAsync()
    {
        if (!CurrentTenant.Id.HasValue)
            return NewDefaultDto();

        var tenantId = CurrentTenant.Id.Value;
        var entity = await FindAsync(tenantId);
        return entity == null ? NewDefaultDto() : ObjectMapper.Map<ShopSetting, ShopSettingDto>(entity);
    }

    [Authorize(EHubPermissions.ShopSettings.Manage)]
    public async Task<ShopSettingDto> CreateOrUpdateAsync(CreateUpdateShopSettingDto input)
    {
        var tenantId = RequireTenant();
        var entity = await FindAsync(tenantId) ?? new ShopSetting(GuidGenerator.Create(), tenantId);
        entity.Update(input.ShopDisplayName, input.LogoFileId, input.Phone, input.AlternatePhone,
            input.Email, input.Website, input.AddressLine1, input.AddressLine2, input.City,
            input.StateOrProvince, input.PostalCode, input.Country, input.CurrencyCode,
            input.CurrencySymbol, input.TaxNumber, input.DefaultTaxPercentage, input.InvoicePrefix,
            input.PurchaseOrderPrefix, input.ReceiptFooter, input.ReturnPolicy, input.AllowNegativeStock,
            input.AutoGenerateProductBarcode, input.AutoGenerateInvoiceQrCode,
            input.DefaultLowStockLevel, input.DecimalPlaces);

        if (entity.CreationTime == default)
            await _repository.InsertAsync(entity, autoSave: true);
        else
            await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<ShopSetting, ShopSettingDto>(entity);
    }

    [Authorize(EHubPermissions.ShopSettings.Default)]
    public async Task<ShopSettingSetupStatusDto> GetSetupStatusAsync()
    {
        if (!CurrentTenant.Id.HasValue)
            return new ShopSettingSetupStatusDto { IsConfigured = false };

        var entity = await FindAsync(CurrentTenant.Id.Value);
        return new ShopSettingSetupStatusDto { IsConfigured = entity?.IsConfigured ?? false, ShopDisplayName = entity?.ShopDisplayName };
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopSetting?> FindAsync(Guid tenantId)
    {
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
    }

    private static ShopSettingDto NewDefaultDto() => new()
    {
        CurrencyCode = "PKR", CurrencySymbol = "₨", InvoicePrefix = "INV", PurchaseOrderPrefix = "PO",
        AutoGenerateProductBarcode = true, AutoGenerateInvoiceQrCode = true, DefaultLowStockLevel = 5, DecimalPlaces = 2
    };
}
