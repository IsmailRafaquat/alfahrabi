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

namespace EHub.ShopManagement.Customers;

[Authorize(EHubPermissions.ShopCustomers.Default)]
public class ShopCustomerAppService : ApplicationService, IShopCustomerAppService
{
    private readonly IRepository<ShopCustomer, Guid> _repository;
    private readonly ShopCustomerManager _manager;

    public ShopCustomerAppService(IRepository<ShopCustomer, Guid> repository, ShopCustomerManager manager)
    {
        _repository = repository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopCustomerDto>> GetListAsync(GetShopCustomersInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopCustomerDto>(0, new List<ShopCustomerDto>());
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
            .WhereIf(input.CustomerType.HasValue, x => x.CustomerType == input.CustomerType)
            .WhereIf(!input.City.IsNullOrWhiteSpace(), x => x.City != null && x.City.Contains(input.City!))
            .WhereIf(!input.Country.IsNullOrWhiteSpace(), x => x.Country != null && x.Country.Contains(input.Country!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive)
            .WhereIf(input.IsWalkInCustomer.HasValue, x => x.IsWalkInCustomer == input.IsWalkInCustomer)
            .WhereIf(input.HasOpeningBalance == true, x => x.OpeningBalance > 0)
            .WhereIf(input.HasOpeningBalance == false, x => x.OpeningBalance == 0)
            .WhereIf(input.HasCreditLimit == true, x => x.CreditLimit > 0)
            .WhereIf(input.HasCreditLimit == false, x => x.CreditLimit == 0);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = ObjectMapper.Map<List<ShopCustomer>, List<ShopCustomerDto>>(entities);
        await HideBalanceIfNotAllowedAsync(items);
        return new PagedResultDto<ShopCustomerDto>(totalCount, items);
    }

    public async Task<ShopCustomerDto> GetAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        var dto = ObjectMapper.Map<ShopCustomer, ShopCustomerDto>(entity);
        await HideBalanceIfNotAllowedAsync(new List<ShopCustomerDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopCustomers.Create)]
    public async Task<ShopCustomerDto> CreateAsync(CreateUpdateShopCustomerDto input)
    {
        var entity = await _manager.CreateAsync(input.Code, input.Name, input.CustomerType, input.ContactPerson,
            input.Phone, input.AlternatePhone, input.Email, input.AddressLine1, input.AddressLine2, input.City,
            input.StateOrProvince, input.PostalCode, input.Country, input.TaxNumber, input.OpeningBalance,
            input.CreditLimit, input.PaymentTermsDays, input.Notes, input.IsWalkInCustomer, input.IsActive);
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCustomers.Edit)]
    public async Task<ShopCustomerDto> UpdateAsync(Guid id, CreateUpdateShopCustomerDto input)
    {
        var entity = await FindEntityAsync(id);
        await _manager.UpdateAsync(entity, input.Code, input.Name, input.CustomerType, input.ContactPerson,
            input.Phone, input.AlternatePhone, input.Email, input.AddressLine1, input.AddressLine2, input.City,
            input.StateOrProvince, input.PostalCode, input.Country, input.TaxNumber, input.OpeningBalance,
            input.CreditLimit, input.PaymentTermsDays, input.Notes, input.IsWalkInCustomer, input.IsActive);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCustomers.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityAsync(id);
        await _manager.ValidateDeleteAsync(entity.Id);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<ListResultDto<ShopCustomerLookupDto>> GetLookupAsync(string? filter = null)
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopCustomerLookupDto>(new List<ShopCustomerLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId && x.IsActive)
            .WhereIf(!filter.IsNullOrWhiteSpace(), x =>
                x.Code.Contains(filter!) ||
                x.Name.Contains(filter!) ||
                (x.ContactPerson != null && x.ContactPerson.Contains(filter!)) ||
                (x.Phone != null && x.Phone.Contains(filter!)));

        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Name));
        var items = ObjectMapper.Map<List<ShopCustomer>, List<ShopCustomerLookupDto>>(entities);
        await HideBalanceIfNotAllowedAsync(items);
        return new ListResultDto<ShopCustomerLookupDto>(items);
    }

    public async Task<ShopCustomerLookupDto?> GetWalkInCustomerAsync()
    {
        if (!CurrentTenant.Id.HasValue) return null;
        var tenantId = CurrentTenant.Id.Value;

        var query = await _repository.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId && x.IsWalkInCustomer && x.IsActive));
        if (entity == null) return null;

        var dto = ObjectMapper.Map<ShopCustomer, ShopCustomerLookupDto>(entity);
        await HideBalanceIfNotAllowedAsync(new List<ShopCustomerLookupDto> { dto });
        return dto;
    }

    private async Task HideBalanceIfNotAllowedAsync(List<ShopCustomerDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomers.ViewBalance)) return;
        foreach (var item in items)
        {
            item.OpeningBalance = null;
            item.CreditLimit = null;
        }
    }

    private async Task HideBalanceIfNotAllowedAsync(List<ShopCustomerLookupDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomers.ViewBalance)) return;
        foreach (var item in items)
        {
            item.CreditLimit = null;
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopCustomer> FindEntityAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:CustomerNotFound");
    }
}
