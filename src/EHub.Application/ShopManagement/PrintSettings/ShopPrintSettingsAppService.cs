using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.PrintSettings;

[Authorize]
public class ShopPrintSettingsAppService : ApplicationService, IShopPrintSettingsAppService
{
    private readonly IRepository<ShopPrintSetting, Guid> _repository;

    public ShopPrintSettingsAppService(IRepository<ShopPrintSetting, Guid> repository) => _repository = repository;

    [Authorize(EHubPermissions.ShopPrint.Default)]
    public async Task<ShopPrintSettingsDto> GetAsync()
    {
        if (!CurrentTenant.Id.HasValue)
            return NewDefaultDto();

        var entity = await FindAsync(CurrentTenant.Id.Value);
        return entity == null ? NewDefaultDto() : ObjectMapper.Map<ShopPrintSetting, ShopPrintSettingsDto>(entity);
    }

    [Authorize(EHubPermissions.ShopPrint.ManageSettings)]
    public async Task<ShopPrintSettingsDto> CreateOrUpdateAsync(CreateUpdateShopPrintSettingsDto input)
    {
        var tenantId = RequireTenant();
        var entity = await FindAsync(tenantId) ?? new ShopPrintSetting(GuidGenerator.Create(), tenantId);
        entity.Update(
            input.DefaultPrintPaperSize,
            input.PrintHeaderLogo, input.PrintShopName, input.PrintShopAddress, input.PrintShopPhone,
            input.PrintShopEmail, input.PrintTaxNumber,
            input.PrintFooterMessage, input.PrintTermsAndConditions,
            input.PrintQrCode, input.PrintBarcode,
            input.PrintCustomerCopyLabel, input.PrintDuplicateCopyLabel,
            input.PrintItemCode, input.PrintUnit, input.PrintBatchNumber, input.PrintExpiryDate,
            input.PrintDiscount, input.PrintTax, input.PrintPaymentDetails, input.PrintCashierName,
            input.PrintDateTime, input.PrintPageNumberForA4,
            input.ThermalFontSize, input.ThermalPrintDensity,
            input.ThermalAutoCut, input.ThermalOpenCashDrawer);

        if (entity.CreationTime == default)
            await _repository.InsertAsync(entity, autoSave: true);
        else
            await _repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<ShopPrintSetting, ShopPrintSettingsDto>(entity);
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopPrintSetting?> FindAsync(Guid tenantId)
    {
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId));
    }

    private static ShopPrintSettingsDto NewDefaultDto() => new();
}
