using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Suppliers;

public class ShopSupplierManager : DomainService
{
    private readonly IRepository<ShopSupplier, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public ShopSupplierManager(IRepository<ShopSupplier, Guid> repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopSupplier> CreateAsync(
        string code,
        string name,
        string? contactPerson,
        string? phone,
        string? alternatePhone,
        string? email,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country,
        string? taxNumber,
        decimal openingBalance,
        decimal creditLimit,
        int paymentTermsDays,
        string? notes,
        bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(name);
        await ValidateUniqueAsync(normalizedCode, normalizedName, tenantId, null);

        return new ShopSupplier(GuidGenerator.Create(), tenantId, normalizedCode, normalizedName, contactPerson, phone,
            alternatePhone, email, addressLine1, addressLine2, city, stateOrProvince, postalCode, country, taxNumber,
            openingBalance, creditLimit, paymentTermsDays, notes, isActive);
    }

    public async Task UpdateAsync(
        ShopSupplier supplier,
        string code,
        string name,
        string? contactPerson,
        string? phone,
        string? alternatePhone,
        string? email,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country,
        string? taxNumber,
        decimal openingBalance,
        decimal creditLimit,
        int paymentTermsDays,
        string? notes,
        bool isActive)
    {
        var tenantId = RequireTenant();
        if (supplier.TenantId != tenantId) throw new BusinessException("ShopManagement:SupplierNotFound");

        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(name);
        await ValidateUniqueAsync(normalizedCode, normalizedName, tenantId, supplier.Id);

        supplier.Update(normalizedCode, normalizedName, contactPerson, phone, alternatePhone, email, addressLine1,
            addressLine2, city, stateOrProvince, postalCode, country, taxNumber, openingBalance, creditLimit,
            paymentTermsDays, notes, isActive);
    }

    public Task ValidateDeleteAsync(Guid id)
    {
        RequireTenant();
        return Task.CompletedTask;
    }

    private async Task ValidateUniqueAsync(string code, string name, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:SupplierCodeAlreadyExists").WithData("Code", code);
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Name == name && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:SupplierNameAlreadyExists").WithData("Name", name);
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private static string NormalizeCode(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopSupplierConsts.CodeMaxLength).Trim();
    private static string NormalizeName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopSupplierConsts.NameMaxLength).Trim();
}
