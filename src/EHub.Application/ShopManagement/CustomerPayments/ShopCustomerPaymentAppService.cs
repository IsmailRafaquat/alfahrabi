using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.CustomerPayments;

[Authorize(EHubPermissions.ShopCustomerPayments.Default)]
public class ShopCustomerPaymentAppService : ApplicationService, IShopCustomerPaymentAppService
{
    private readonly IRepository<ShopCustomerPayment, Guid> _repository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly ShopCustomerPaymentManager _manager;

    public ShopCustomerPaymentAppService(
        IRepository<ShopCustomerPayment, Guid> repository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopSale, Guid> saleRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        ShopCustomerPaymentManager manager)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _saleRepository = saleRepository;
        _bankAccountRepository = bankAccountRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopCustomerPaymentDto>> GetListAsync(GetShopCustomerPaymentsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopCustomerPaymentDto>(0, new List<ShopCustomerPaymentDto>());
        var tenantId = CurrentTenant.Id.Value;

        var payments = await _repository.GetQueryableAsync();
        var customers = await _customerRepository.GetQueryableAsync();

        var filtered = payments.Where(x => x.TenantId == tenantId)
            .WhereIf(input.CustomerId.HasValue, x => x.CustomerId == input.CustomerId)
            .WhereIf(input.PaymentType.HasValue, x => x.PaymentType == input.PaymentType)
            .WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.PaymentDateFrom.HasValue, x => x.PaymentDate >= input.PaymentDateFrom!.Value)
            .WhereIf(input.PaymentDateTo.HasValue, x => x.PaymentDate <= input.PaymentDateTo!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingCustomerIds = await AsyncExecuter.ToListAsync(customers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.PaymentNumber.Contains(input.Filter!) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.Filter!)) ||
                (x.ChequeNumber != null && x.ChequeNumber.Contains(input.Filter!)) ||
                matchingCustomerIds.Contains(x.CustomerId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "PaymentDate desc, CreationTime desc" : input.Sorting!;
        var pagedPayments = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from payment in pagedPayments
                        join customer in customers on payment.CustomerId equals customer.Id
                        select new ShopCustomerPaymentDto
                        {
                            Id = payment.Id,
                            PaymentNumber = payment.PaymentNumber,
                            CustomerId = payment.CustomerId,
                            CustomerCode = customer.Code,
                            CustomerName = customer.Name,
                            PaymentDate = payment.PaymentDate,
                            PaymentType = payment.PaymentType,
                            PaymentMethod = payment.PaymentMethod,
                            Amount = payment.Amount,
                            ReferenceNumber = payment.ReferenceNumber,
                            ChequeNumber = payment.ChequeNumber,
                            BankName = payment.BankName,
                            BankAccountId = payment.BankAccountId,
                            Notes = payment.Notes,
                            Status = payment.Status,
                            PostedByUserId = payment.PostedByUserId,
                            PostedDate = payment.PostedDate,
                            CancelledByUserId = payment.CancelledByUserId,
                            CancelledDate = payment.CancelledDate,
                            CancellationReason = payment.CancellationReason,
                            CreationTime = payment.CreationTime,
                            Allocations = new List<ShopCustomerPaymentAllocationDto>()
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await PopulateAllocatedAmountsAsync(items);
        await PopulateBankAccountNamesAsync(items);
        await HideAmountIfNotAllowedAsync(items);
        return new PagedResultDto<ShopCustomerPaymentDto>(totalCount, items);
    }

    public async Task<ShopCustomerPaymentDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        var dto = await MapToDtoAsync(entity);
        await HideAmountIfNotAllowedAsync(new List<ShopCustomerPaymentDto> { dto });
        return dto;
    }

    public async Task<ListResultDto<ShopCustomerOutstandingSaleDto>> GetOutstandingSalesAsync(Guid customerId)
    {
        var tenantId = RequireTenant();

        var customerQuery = await _customerRepository.GetQueryableAsync();
        var customerExists = await AsyncExecuter.AnyAsync(customerQuery.Where(x => x.Id == customerId && x.TenantId == tenantId));
        if (!customerExists) throw new BusinessException("ShopManagement:CustomerPaymentCustomerNotFound");

        var saleQuery = await _saleRepository.GetQueryableAsync();
        var completedSales = saleQuery
            .Where(x => x.TenantId == tenantId && x.CustomerId == customerId && x.Status == ShopSaleStatus.Completed)
            .ToList();
        var saleIds = completedSales.Select(x => x.Id).ToList();

        var postedAllocated = await _manager.GetPostedAllocatedAmountsAsync(tenantId, saleIds, excludePaymentId: null);
        var canViewAmount = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomerPayments.ViewAmount);

        var result = new List<ShopCustomerOutstandingSaleDto>();
        foreach (var sale in completedSales)
        {
            var additionalPaid = postedAllocated.TryGetValue(sale.Id, out var value) ? value : 0;
            var totalPaid = sale.PaidAmount + additionalPaid;
            var pending = Math.Max(0, sale.GrandTotal - totalPaid);
            if (pending <= 0) continue;

            result.Add(new ShopCustomerOutstandingSaleDto
            {
                SaleId = sale.Id,
                SaleNumber = sale.SaleNumber,
                SaleDate = sale.SaleDate,
                DueDate = sale.DueDate,
                GrandTotal = canViewAmount ? sale.GrandTotal : null,
                InitialPaidAmount = canViewAmount ? sale.PaidAmount : null,
                AdditionalPaidAmount = canViewAmount ? additionalPaid : null,
                TotalPaidAmount = canViewAmount ? totalPaid : null,
                PendingAmount = canViewAmount ? pending : null
            });
        }

        return new ListResultDto<ShopCustomerOutstandingSaleDto>(result.OrderBy(x => x.SaleDate).ToList());
    }

    [Authorize(EHubPermissions.ShopCustomerPayments.Create)]
    public async Task<ShopCustomerPaymentDto> CreateAsync(CreateUpdateShopCustomerPaymentDto input)
    {
        var entity = await _manager.CreateAsync(input.CustomerId, input.PaymentDate, input.PaymentType, input.PaymentMethod,
            input.Amount, input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Notes, ToAllocationInputs(input.Allocations));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCustomerPayments.Edit)]
    public async Task<ShopCustomerPaymentDto> UpdateAsync(Guid id, CreateUpdateShopCustomerPaymentDto input)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        if (input.CustomerId != entity.CustomerId) throw new BusinessException("ShopManagement:CustomerPaymentCustomerCannotBeChanged");

        await _manager.UpdateAsync(entity, input.PaymentDate, input.PaymentType, input.PaymentMethod, input.Amount,
            input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Notes, ToAllocationInputs(input.Allocations));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCustomerPayments.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopCustomerPayments.Post)]
    public async Task<ShopCustomerPaymentDto> PostAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCustomerPayments.Cancel)]
    public async Task<ShopCustomerPaymentDto> CancelAsync(Guid id, CancelShopCustomerPaymentDto input)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopCustomerPaymentAllocationInput> ToAllocationInputs(IEnumerable<CreateUpdateShopCustomerPaymentAllocationDto> allocations) =>
        allocations.Select(x => new ShopCustomerPaymentAllocationInput
        {
            SaleId = x.SaleId,
            AllocatedAmount = x.AllocatedAmount
        }).ToList();

    private async Task PopulateAllocatedAmountsAsync(List<ShopCustomerPaymentDto> items)
    {
        if (items.Count == 0) return;

        var paymentIds = items.Select(x => x.Id).ToList();
        var query = await _repository.GetQueryableAsync();
        var grouped = query
            .Where(p => paymentIds.Contains(p.Id))
            .SelectMany(p => p.Allocations)
            .GroupBy(a => a.CustomerPaymentId)
            .Select(g => new { PaymentId = g.Key, Allocated = g.Sum(a => a.AllocatedAmount) });

        var sums = (await AsyncExecuter.ToListAsync(grouped)).ToDictionary(x => x.PaymentId, x => x.Allocated);

        foreach (var item in items)
        {
            var allocated = sums.TryGetValue(item.Id, out var value) ? value : 0;
            item.AllocatedAmount = allocated;
            item.UnallocatedAmount = (item.Amount ?? 0) - allocated;
        }
    }

    private async Task HideAmountIfNotAllowedAsync(List<ShopCustomerPaymentDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCustomerPayments.ViewAmount)) return;
        foreach (var dto in items)
        {
            dto.Amount = null;
            dto.AllocatedAmount = null;
            dto.UnallocatedAmount = null;
            foreach (var allocation in dto.Allocations)
            {
                allocation.GrandTotal = null;
                allocation.AllocatedAmount = null;
            }
        }
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopCustomerPayment> FindEntityWithAllocationsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Allocations, x => x.Customer!);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:CustomerPaymentNotFound");
    }

    private async Task<ShopCustomerPaymentDto> MapToDtoAsync(ShopCustomerPayment entity)
    {
        var saleIds = entity.Allocations.Select(x => x.SaleId).Distinct().ToList();
        var saleQuery = await _saleRepository.GetQueryableAsync();
        var sales = saleQuery.Where(x => saleIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var allocatedAmount = entity.Allocations.Sum(x => x.AllocatedAmount);

        var dto = new ShopCustomerPaymentDto
        {
            Id = entity.Id,
            PaymentNumber = entity.PaymentNumber,
            CustomerId = entity.CustomerId,
            CustomerCode = entity.Customer?.Code ?? string.Empty,
            CustomerName = entity.Customer?.Name ?? string.Empty,
            PaymentDate = entity.PaymentDate,
            PaymentType = entity.PaymentType,
            PaymentMethod = entity.PaymentMethod,
            Amount = entity.Amount,
            ReferenceNumber = entity.ReferenceNumber,
            ChequeNumber = entity.ChequeNumber,
            BankName = entity.BankName,
            BankAccountId = entity.BankAccountId,
            Notes = entity.Notes,
            Status = entity.Status,
            AllocatedAmount = allocatedAmount,
            UnallocatedAmount = entity.Amount - allocatedAmount,
            PostedByUserId = entity.PostedByUserId,
            PostedDate = entity.PostedDate,
            CancelledByUserId = entity.CancelledByUserId,
            CancelledDate = entity.CancelledDate,
            CancellationReason = entity.CancellationReason,
            CreationTime = entity.CreationTime,
            Allocations = entity.Allocations.Select(x =>
            {
                sales.TryGetValue(x.SaleId, out var sale);
                return new ShopCustomerPaymentAllocationDto
                {
                    Id = x.Id,
                    SaleId = x.SaleId,
                    SaleNumber = sale?.SaleNumber ?? string.Empty,
                    SaleDate = sale?.SaleDate ?? default,
                    GrandTotal = sale?.GrandTotal,
                    AllocatedAmount = x.AllocatedAmount,
                    CreationTime = x.CreationTime
                };
            }).ToList()
        };
        await PopulateBankAccountNamesAsync(new List<ShopCustomerPaymentDto> { dto });
        return dto;
    }

    private async Task PopulateBankAccountNamesAsync(List<ShopCustomerPaymentDto> items)
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
}
