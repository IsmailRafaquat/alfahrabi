using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.Units;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.SaleReturns;

[Authorize(EHubPermissions.ShopSaleReturns.Default)]
public class ShopSaleReturnAppService : ApplicationService, IShopSaleReturnAppService
{
    private readonly IRepository<ShopSaleReturn, Guid> _repository;
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly ShopSaleReturnManager _manager;

    public ShopSaleReturnAppService(
        IRepository<ShopSaleReturn, Guid> repository,
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        ShopSaleReturnManager manager)
    {
        _repository = repository;
        _saleRepository = saleRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _bankAccountRepository = bankAccountRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopSaleReturnDto>> GetListAsync(GetShopSaleReturnsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopSaleReturnDto>(0, new List<ShopSaleReturnDto>());
        var tenantId = CurrentTenant.Id.Value;

        var returns = await _repository.GetQueryableAsync();
        var sales = await _saleRepository.GetQueryableAsync();
        var customers = await _customerRepository.GetQueryableAsync();

        var filtered = returns.Where(x => x.TenantId == tenantId)
            .WhereIf(input.SaleId.HasValue, x => x.SaleId == input.SaleId)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.Reason.HasValue, x => x.Reason == input.Reason)
            .WhereIf(input.SettlementType.HasValue, x => x.SettlementType == input.SettlementType)
            .WhereIf(input.ReturnDateFrom.HasValue, x => x.ReturnDate >= input.ReturnDateFrom!.Value)
            .WhereIf(input.ReturnDateTo.HasValue, x => x.ReturnDate <= input.ReturnDateTo!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingCustomerIds = await AsyncExecuter.ToListAsync(customers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));
            var matchingSaleIds = await AsyncExecuter.ToListAsync(sales
                .Where(x => x.TenantId == tenantId && x.SaleNumber.Contains(input.Filter!))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.SaleReturnNumber.Contains(input.Filter!) ||
                matchingCustomerIds.Contains(x.CustomerId) ||
                matchingSaleIds.Contains(x.SaleId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "ReturnDate desc, CreationTime desc" : input.Sorting!;
        var pagedReturns = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var rows = await AsyncExecuter.ToListAsync(pagedReturns);
        var saleIds = rows.Select(x => x.SaleId).Distinct().ToList();
        var customerIds = rows.Select(x => x.CustomerId).Distinct().ToList();
        var saleMap = sales.Where(x => saleIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var customerMap = customers.Where(x => customerIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var dtos = rows.Select(x => MapHeader(x, saleMap.GetValueOrDefault(x.SaleId), customerMap.GetValueOrDefault(x.CustomerId))).ToList();
        await PopulateBankAccountNamesAsync(dtos);
        await HidePriceIfNotAllowedAsync(dtos);
        await HideCostIfNotAllowedAsync(dtos);
        return new PagedResultDto<ShopSaleReturnDto>(totalCount, dtos);
    }

    public async Task<ShopSaleReturnDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        var dto = await MapToDtoAsync(entity);
        var items = new List<ShopSaleReturnDto> { dto };
        await HidePriceIfNotAllowedAsync(items);
        await HideCostIfNotAllowedAsync(items);
        return dto;
    }

    public async Task<ShopSaleReturnableDto> GetSaleForReturnAsync(Guid saleId)
    {
        var tenantId = RequireTenant();
        var sale = await _manager.GetCompletedSaleAsync(saleId, tenantId);
        var customer = await _customerRepository.GetAsync(sale.CustomerId);

        var canViewPrice = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSaleReturns.ViewPrice);

        var dto = new ShopSaleReturnableDto
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            CustomerId = customer.Id,
            CustomerCode = customer.Code,
            CustomerName = customer.Name,
            SaleDate = sale.SaleDate,
            GrandTotal = canViewPrice ? sale.GrandTotal : null,
            PaidAmount = canViewPrice ? sale.PaidAmount : null,
            PendingAmount = canViewPrice ? sale.PendingAmount : null
        };

        var productIds = sale.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);
        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        foreach (var item in sale.Items)
        {
            var previouslyReturned = await _manager.GetReturnedQuantityAsync(item.Id, tenantId);
            var returnable = item.Quantity - previouslyReturned;
            if (returnable <= 0) continue;

            products.TryGetValue(item.ProductId, out var product);
            ShopUnit? unit = null;
            if (product != null) units.TryGetValue(product.UnitId, out unit);

            dto.Items.Add(new ShopSaleReturnableItemDto
            {
                Id = item.Id,
                SaleItemId = item.Id,
                ProductId = item.ProductId,
                ProductCode = item.ProductCodeSnapshot,
                ProductName = item.ProductNameSnapshot,
                UnitName = item.UnitNameSnapshot,
                UnitShortName = item.UnitShortNameSnapshot,
                UnitAllowDecimal = unit?.AllowDecimal ?? false,
                SoldQuantity = item.Quantity,
                PreviouslyReturnedQuantity = previouslyReturned,
                ReturnableQuantity = returnable,
                UnitSalePrice = canViewPrice ? item.UnitSalePrice : null,
                DiscountPercentage = item.DiscountPercentage,
                TaxPercentage = item.TaxPercentage,
                BatchNumber = item.BatchNumber,
                ExpiryDate = item.ExpiryDate
            });
        }

        return dto;
    }

    [Authorize(EHubPermissions.ShopSaleReturns.Create)]
    public async Task<ShopSaleReturnDto> CreateAsync(CreateShopSaleReturnDto input)
    {
        var entity = await _manager.CreateAsync(input.SaleId, input.ReturnDate, input.Reason, input.ReasonDetails,
            input.SettlementType, input.BankAccountId, input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSaleReturns.Edit)]
    public async Task<ShopSaleReturnDto> UpdateAsync(Guid id, UpdateShopSaleReturnDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.UpdateAsync(entity, input.ReturnDate, input.Reason, input.ReasonDetails, input.SettlementType,
            input.BankAccountId, input.OtherCharges, input.Notes, ToItemInputs(input.Items));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSaleReturns.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopSaleReturns.Complete)]
    public async Task<ShopSaleReturnDto> CompleteAsync(Guid id)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CompleteAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSaleReturns.Cancel)]
    public async Task<ShopSaleReturnDto> CancelAsync(Guid id, CancelShopSaleReturnDto input)
    {
        var entity = await FindEntityWithItemsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopSaleReturnItemInput> ToItemInputs(IEnumerable<CreateShopSaleReturnItemDto> items) =>
        items.Select(x => new ShopSaleReturnItemInput
        {
            SaleItemId = x.SaleItemId,
            ReturnQuantity = x.ReturnQuantity,
            Reason = x.Reason,
            Notes = x.Notes
        }).ToList();

    private ShopSaleReturnDto MapHeader(ShopSaleReturn entity, ShopSale? sale, ShopCustomer? customer) => new()
    {
        Id = entity.Id,
        SaleReturnNumber = entity.SaleReturnNumber,
        SaleId = entity.SaleId,
        SaleNumber = sale?.SaleNumber ?? string.Empty,
        CustomerId = entity.CustomerId,
        CustomerCode = customer?.Code ?? string.Empty,
        CustomerName = customer?.Name ?? string.Empty,
        ReturnDate = entity.ReturnDate,
        Status = entity.Status,
        Reason = entity.Reason,
        ReasonDetails = entity.ReasonDetails,
        SettlementType = entity.SettlementType,
        BankAccountId = entity.BankAccountId,
        SubTotal = entity.SubTotal,
        DiscountAmount = entity.DiscountAmount,
        TaxAmount = entity.TaxAmount,
        OtherCharges = entity.OtherCharges,
        GrandTotal = entity.GrandTotal,
        RefundAmount = entity.RefundAmount,
        CustomerCreditAmount = entity.CustomerCreditAmount,
        Notes = entity.Notes,
        CompletedDate = entity.CompletedDate,
        CancelledDate = entity.CancelledDate,
        CancellationReason = entity.CancellationReason,
        CreationTime = entity.CreationTime,
        Items = new List<ShopSaleReturnItemDto>()
    };

    private async Task<ShopSaleReturnDto> MapToDtoAsync(ShopSaleReturn entity)
    {
        var sale = await AsyncExecuter.FirstOrDefaultAsync((await _saleRepository.GetQueryableAsync()).Where(x => x.Id == entity.SaleId));
        var customer = await AsyncExecuter.FirstOrDefaultAsync((await _customerRepository.GetQueryableAsync()).Where(x => x.Id == entity.CustomerId));

        var dto = MapHeader(entity, sale, customer);
        dto.Items = entity.Items.Select(x => new ShopSaleReturnItemDto
        {
            Id = x.Id,
            SaleItemId = x.SaleItemId,
            ProductId = x.ProductId,
            ProductCode = x.ProductCodeSnapshot,
            ProductName = x.ProductNameSnapshot,
            UnitName = x.UnitNameSnapshot,
            UnitShortName = x.UnitShortNameSnapshot,
            SoldQuantity = x.SoldQuantitySnapshot,
            PreviouslyReturnedQuantity = x.PreviouslyReturnedQuantity,
            ReturnableQuantity = x.SoldQuantitySnapshot - x.PreviouslyReturnedQuantity,
            ReturnQuantity = x.ReturnQuantity,
            UnitSalePrice = x.UnitSalePrice,
            UnitCostSnapshot = x.UnitCostSnapshot,
            DiscountPercentage = x.DiscountPercentage,
            DiscountAmount = x.DiscountAmount,
            TaxPercentage = x.TaxPercentage,
            TaxAmount = x.TaxAmount,
            LineSubTotal = x.LineSubTotal,
            LineTotal = x.LineTotal,
            BatchNumber = x.BatchNumber,
            ExpiryDate = x.ExpiryDate,
            Reason = x.Reason,
            Notes = x.Notes
        }).ToList();

        await PopulateBankAccountNamesAsync(new List<ShopSaleReturnDto> { dto });
        return dto;
    }

    private async Task PopulateBankAccountNamesAsync(List<ShopSaleReturnDto> items)
    {
        var bankAccountIds = items.Where(x => x.BankAccountId.HasValue).Select(x => x.BankAccountId!.Value).Distinct().ToList();
        if (bankAccountIds.Count == 0) return;

        var query = await _bankAccountRepository.GetQueryableAsync();
        var accounts = query.Where(x => bankAccountIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        foreach (var item in items)
        {
            if (item.BankAccountId.HasValue && accounts.TryGetValue(item.BankAccountId.Value, out var account))
            {
                item.BankAccountCode = account.Code;
                item.BankAccountName = account.AccountName;
            }
        }
    }

    private async Task HidePriceIfNotAllowedAsync(List<ShopSaleReturnDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSaleReturns.ViewPrice)) return;
        foreach (var dto in items)
        {
            dto.SubTotal = null;
            dto.DiscountAmount = null;
            dto.TaxAmount = null;
            dto.OtherCharges = null;
            dto.GrandTotal = null;
            dto.RefundAmount = null;
            dto.CustomerCreditAmount = null;
            foreach (var item in dto.Items)
            {
                item.UnitSalePrice = null;
                item.DiscountAmount = null;
                item.TaxAmount = null;
                item.LineSubTotal = null;
                item.LineTotal = null;
            }
        }
    }

    private async Task HideCostIfNotAllowedAsync(List<ShopSaleReturnDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSaleReturns.ViewCost)) return;
        foreach (var dto in items)
            foreach (var item in dto.Items)
                item.UnitCostSnapshot = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopSaleReturn> FindEntityWithItemsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:SaleReturnNotFound");
    }
}
