using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.Suppliers;

[Authorize(EHubPermissions.ShopSuppliers.Default)]
public class ShopSupplierAppService : ApplicationService, IShopSupplierAppService
{
    private readonly IRepository<ShopSupplier, Guid> _repository;
    private readonly ShopSupplierManager _manager;

    public ShopSupplierAppService(IRepository<ShopSupplier, Guid> repository, ShopSupplierManager manager)
    {
        _repository = repository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopSupplierDto>> GetListAsync(GetShopSuppliersInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopSupplierDto>(0, new List<ShopSupplierDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.Code.Contains(input.Filter!) ||
                x.Name.Contains(input.Filter!) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(input.Filter!)) ||
                (x.Phone != null && x.Phone.Contains(input.Filter!)) ||
                (x.AlternatePhone != null && x.AlternatePhone.Contains(input.Filter!)) ||
                (x.Email != null && x.Email.Contains(input.Filter!)) ||
                (x.TaxNumber != null && x.TaxNumber.Contains(input.Filter!)))
            .WhereIf(!input.City.IsNullOrWhiteSpace(), x => x.City != null && x.City.Contains(input.City!))
            .WhereIf(!input.Country.IsNullOrWhiteSpace(), x => x.Country != null && x.Country.Contains(input.Country!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.HasOpeningBalance == true, x => x.OpeningBalance > 0)
            .WhereIf(input.HasOpeningBalance == false, x => x.OpeningBalance == 0)
            .WhereIf(input.HasCreditLimit == true, x => x.CreditLimit > 0)
            .WhereIf(input.HasCreditLimit == false, x => x.CreditLimit == 0);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = ObjectMapper.Map<List<ShopSupplier>, List<ShopSupplierDto>>(entities);
        await HideBalanceIfNotAllowedAsync(items);
        return new PagedResultDto<ShopSupplierDto>(totalCount, items);
    }

    public async Task<ShopSupplierDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dto = ObjectMapper.Map<ShopSupplier, ShopSupplierDto>(entity);
        await HideBalanceIfNotAllowedAsync(new List<ShopSupplierDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopSuppliers.Create)]
    public async Task<ShopSupplierDto> CreateAsync(CreateUpdateShopSupplierDto input)
    {
        var entity = await _manager.CreateAsync(input.Code, input.Name, input.ContactPerson, input.Phone,
            input.AlternatePhone, input.Email, input.AddressLine1, input.AddressLine2, input.City,
            input.StateOrProvince, input.PostalCode, input.Country, input.TaxNumber, input.OpeningBalance,
            input.CreditLimit, input.PaymentTermsDays, input.Notes, input.IsActive);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSuppliers.Edit)]
    public async Task<ShopSupplierDto> UpdateAsync(Guid id, CreateUpdateShopSupplierDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.Code, input.Name, input.ContactPerson, input.Phone,
            input.AlternatePhone, input.Email, input.AddressLine1, input.AddressLine2, input.City,
            input.StateOrProvince, input.PostalCode, input.Country, input.TaxNumber, input.OpeningBalance,
            input.CreditLimit, input.PaymentTermsDays, input.Notes, input.IsActive);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSuppliers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity.Id);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopSupplierLookupDto>> GetLookupAsync(string? filter = null)
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopSupplierLookupDto>(new List<ShopSupplierLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId && x.IsActive)
            .WhereIf(!filter.IsNullOrWhiteSpace(), x =>
                x.Code.Contains(filter!) ||
                x.Name.Contains(filter!) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(filter!)) ||
                (x.Phone != null && x.Phone.Contains(filter!)));

        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Name));
        var items = ObjectMapper.Map<List<ShopSupplier>, List<ShopSupplierLookupDto>>(entities);
        return new ListResultDto<ShopSupplierLookupDto>(items);
    }

    private async Task HideBalanceIfNotAllowedAsync(List<ShopSupplierDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSuppliers.ViewBalance)) return;
        foreach (var item in items)
        {
            item.OpeningBalance = null;
            item.CreditLimit = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopSupplier> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:SupplierNotFound");
    }
}
