using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Customers;

public class ShopCustomerManager : DomainService
{
    private readonly IRepository<ShopCustomer, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;

    public ShopCustomerManager(IRepository<ShopCustomer, Guid> repository, ICurrentTenant currentTenant)
    {
        _repository = repository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopCustomer> CreateAsync(
        string code,
        string name,
        ShopCustomerType customerType,
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
        bool isWalkInCustomer,
        bool isActive)
    {
        var tenantId = RequireTenant();
        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(name);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, null);
        if (isWalkInCustomer) await ValidateNoWalkInCustomerAsync(tenantId, null);

        return new ShopCustomer(GuidGenerator.Create(), tenantId, normalizedCode, normalizedName, customerType,
            contactPerson, phone, alternatePhone, email, addressLine1, addressLine2, city, stateOrProvince,
            postalCode, country, taxNumber, openingBalance, creditLimit, paymentTermsDays, notes,
            isWalkInCustomer, isActive);
    }

    public async Task UpdateAsync(
        ShopCustomer customer,
        string code,
        string name,
        ShopCustomerType customerType,
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
        bool isWalkInCustomer,
        bool isActive)
    {
        var tenantId = RequireTenant();
        if (customer.TenantId != tenantId) throw new BusinessException("ShopManagement:CustomerNotFound");

        var normalizedCode = NormalizeCode(code);
        var normalizedName = NormalizeName(name);
        await ValidateUniqueCodeAsync(normalizedCode, tenantId, customer.Id);
        if (isWalkInCustomer) await ValidateNoWalkInCustomerAsync(tenantId, customer.Id);

        customer.Update(normalizedCode, normalizedName, customerType, contactPerson, phone, alternatePhone, email,
            addressLine1, addressLine2, city, stateOrProvince, postalCode, country, taxNumber, openingBalance,
            creditLimit, paymentTermsDays, notes, isWalkInCustomer, isActive);
    }

    public Task ValidateDeleteAsync(Guid id)
    {
        RequireTenant();
        // Extensibility point: once Sales / Customer Payments exist, check here whether
        // this customer has any historical transactions before allowing deletion.
        return Task.CompletedTask;
    }

    private async Task ValidateUniqueCodeAsync(string code, Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.Code == code && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:CustomerCodeAlreadyExists").WithData("Code", code);
    }

    private async Task ValidateNoWalkInCustomerAsync(Guid tenantId, Guid? excludedId)
    {
        var query = await _repository.GetQueryableAsync();
        if (await AsyncExecuter.AnyAsync(query.Where(x => x.TenantId == tenantId && x.IsWalkInCustomer && (!excludedId.HasValue || x.Id != excludedId))))
            throw new BusinessException("ShopManagement:WalkInCustomerAlreadyExists");
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
    private static string NormalizeCode(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopCustomerConsts.CodeMaxLength).Trim();
    private static string NormalizeName(string value) => Check.NotNullOrWhiteSpace(value, nameof(value), ShopCustomerConsts.NameMaxLength).Trim();
}
