using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.Suppliers;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.SupplierPayments;

[Authorize(EHubPermissions.ShopSupplierPayments.Default)]
public class ShopSupplierPaymentAppService : ApplicationService, IShopSupplierPaymentAppService
{
    private readonly IRepository<ShopSupplierPayment, Guid> _repository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly IRepository<ShopBankAccount, Guid> _bankAccountRepository;
    private readonly ShopSupplierPaymentManager _manager;

    public ShopSupplierPaymentAppService(
        IRepository<ShopSupplierPayment, Guid> repository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        IRepository<ShopBankAccount, Guid> bankAccountRepository,
        ShopSupplierPaymentManager manager)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _bankAccountRepository = bankAccountRepository;
        _manager = manager;
    }

    public async Task<PagedResultDto<ShopSupplierPaymentDto>> GetListAsync(GetShopSupplierPaymentsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopSupplierPaymentDto>(0, new List<ShopSupplierPaymentDto>());
        var tenantId = CurrentTenant.Id.Value;

        var payments = await _repository.GetQueryableAsync();
        var suppliers = await _supplierRepository.GetQueryableAsync();

        var filtered = payments.Where(x => x.TenantId == tenantId)
            .WhereIf(input.SupplierId.HasValue, x => x.SupplierId == input.SupplierId)
            .WhereIf(input.PaymentType.HasValue, x => x.PaymentType == input.PaymentType)
            .WhereIf(input.PaymentMethod.HasValue, x => x.PaymentMethod == input.PaymentMethod)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status)
            .WhereIf(input.PaymentDateFrom.HasValue, x => x.PaymentDate >= input.PaymentDateFrom!.Value)
            .WhereIf(input.PaymentDateTo.HasValue, x => x.PaymentDate <= input.PaymentDateTo!.Value);

        if (!input.Filter.IsNullOrWhiteSpace())
        {
            var matchingSupplierIds = await AsyncExecuter.ToListAsync(suppliers
                .Where(x => x.TenantId == tenantId && (x.Name.Contains(input.Filter!) || x.Code.Contains(input.Filter!)))
                .Select(x => x.Id));

            filtered = filtered.Where(x =>
                x.PaymentNumber.Contains(input.Filter!) ||
                (x.ReferenceNumber != null && x.ReferenceNumber.Contains(input.Filter!)) ||
                (x.ChequeNumber != null && x.ChequeNumber.Contains(input.Filter!)) ||
                matchingSupplierIds.Contains(x.SupplierId));
        }

        var totalCount = await AsyncExecuter.CountAsync(filtered);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "PaymentDate desc, CreationTime desc" : input.Sorting!;
        var pagedPayments = filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount);

        var projected = from payment in pagedPayments
                        join supplier in suppliers on payment.SupplierId equals supplier.Id
                        select new ShopSupplierPaymentDto
                        {
                            Id = payment.Id,
                            PaymentNumber = payment.PaymentNumber,
                            SupplierId = payment.SupplierId,
                            SupplierCode = supplier.Code,
                            SupplierName = supplier.Name,
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
                            Allocations = new List<ShopSupplierPaymentAllocationDto>()
                        };

        var items = await AsyncExecuter.ToListAsync(projected);
        await PopulateAllocatedAmountsAsync(items);
        await PopulateBankAccountNamesAsync(items);
        await HideAmountIfNotAllowedAsync(items);
        return new PagedResultDto<ShopSupplierPaymentDto>(totalCount, items);
    }

    public async Task<ShopSupplierPaymentDto> GetAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        var dto = await MapToDtoAsync(entity);
        await HideAmountIfNotAllowedAsync(new List<ShopSupplierPaymentDto> { dto });
        return dto;
    }

    public async Task<List<ShopSupplierOutstandingReceiptDto>> GetOutstandingReceiptsAsync(Guid supplierId)
    {
        var tenantId = RequireTenant();

        var supplierQuery = await _supplierRepository.GetQueryableAsync();
        var supplierExists = await AsyncExecuter.AnyAsync(supplierQuery.Where(x => x.Id == supplierId && x.TenantId == tenantId));
        if (!supplierExists) throw new BusinessException("ShopManagement:SupplierPaymentSupplierNotFound");

        var grQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var completedReceipts = grQuery
            .Where(x => x.TenantId == tenantId && x.SupplierId == supplierId && x.Status == ShopGoodsReceiptStatus.Completed)
            .ToList();
        var receiptIds = completedReceipts.Select(x => x.Id).ToList();

        var postedAllocated = await _manager.GetPostedAllocatedAmountsAsync(tenantId, receiptIds, excludePaymentId: null);
        var canViewAmount = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSupplierPayments.ViewAmount);

        var result = new List<ShopSupplierOutstandingReceiptDto>();
        foreach (var gr in completedReceipts)
        {
            var paid = postedAllocated.TryGetValue(gr.Id, out var value) ? value : 0;
            var pending = gr.GrandTotal - paid;
            if (pending <= 0) continue;

            result.Add(new ShopSupplierOutstandingReceiptDto
            {
                GoodsReceiptId = gr.Id,
                GoodsReceiptNumber = gr.GoodsReceiptNumber,
                SupplierInvoiceNumber = gr.SupplierInvoiceNumber,
                ReceiptDate = gr.ReceiptDate,
                GrandTotal = canViewAmount ? gr.GrandTotal : null,
                PaidAmount = canViewAmount ? paid : null,
                PendingAmount = canViewAmount ? pending : null
            });
        }

        return result.OrderBy(x => x.ReceiptDate).ToList();
    }

    [Authorize(EHubPermissions.ShopSupplierPayments.Create)]
    public async Task<ShopSupplierPaymentDto> CreateAsync(CreateUpdateShopSupplierPaymentDto input)
    {
        var entity = await _manager.CreateAsync(input.SupplierId, input.PaymentDate, input.PaymentType, input.PaymentMethod,
            input.Amount, input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Notes, ToAllocationInputs(input.Allocations));
        await _repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSupplierPayments.Edit)]
    public async Task<ShopSupplierPaymentDto> UpdateAsync(Guid id, CreateUpdateShopSupplierPaymentDto input)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        if (input.SupplierId != entity.SupplierId) throw new BusinessException("ShopManagement:SupplierPaymentSupplierCannotBeChanged");

        await _manager.UpdateAsync(entity, input.PaymentDate, input.PaymentType, input.PaymentMethod, input.Amount,
            input.ReferenceNumber, input.ChequeNumber, input.BankName, input.BankAccountId, input.Notes, ToAllocationInputs(input.Allocations));
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSupplierPayments.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _repository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopSupplierPayments.Post)]
    public async Task<ShopSupplierPaymentDto> PostAsync(Guid id)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.PostAsync(entity);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopSupplierPayments.Cancel)]
    public async Task<ShopSupplierPaymentDto> CancelAsync(Guid id, CancelShopSupplierPaymentDto input)
    {
        var entity = await FindEntityWithAllocationsAsync(id);
        await _manager.CancelAsync(entity, input.CancellationReason);
        await _repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    private static List<ShopSupplierPaymentAllocationInput> ToAllocationInputs(IEnumerable<CreateUpdateShopSupplierPaymentAllocationDto> allocations) =>
        allocations.Select(x => new ShopSupplierPaymentAllocationInput
        {
            GoodsReceiptId = x.GoodsReceiptId,
            AllocatedAmount = x.AllocatedAmount
        }).ToList();

    private async Task PopulateAllocatedAmountsAsync(List<ShopSupplierPaymentDto> items)
    {
        if (items.Count == 0) return;

        var paymentIds = items.Select(x => x.Id).ToList();
        var query = await _repository.GetQueryableAsync();
        var grouped = query
            .Where(p => paymentIds.Contains(p.Id))
            .SelectMany(p => p.Allocations)
            .GroupBy(a => a.SupplierPaymentId)
            .Select(g => new { PaymentId = g.Key, Allocated = g.Sum(a => a.AllocatedAmount) });

        var sums = (await AsyncExecuter.ToListAsync(grouped)).ToDictionary(x => x.PaymentId, x => x.Allocated);

        foreach (var item in items)
        {
            var allocated = sums.TryGetValue(item.Id, out var value) ? value : 0;
            item.AllocatedAmount = allocated;
            item.UnallocatedAmount = (item.Amount ?? 0) - allocated;
        }
    }

    private async Task HideAmountIfNotAllowedAsync(List<ShopSupplierPaymentDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopSupplierPayments.ViewAmount)) return;
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

    private async Task<ShopSupplierPayment> FindEntityWithAllocationsAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _repository.WithDetailsAsync(x => x.Allocations, x => x.Supplier!);
        var entity = query.FirstOrDefault(x => x.Id == id && x.TenantId == tenantId);
        return entity ?? throw new BusinessException("ShopManagement:SupplierPaymentNotFound");
    }

    private async Task<ShopSupplierPaymentDto> MapToDtoAsync(ShopSupplierPayment entity)
    {
        var goodsReceiptIds = entity.Allocations.Select(x => x.GoodsReceiptId).Distinct().ToList();
        var grQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var goodsReceipts = grQuery.Where(x => goodsReceiptIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var allocatedAmount = entity.Allocations.Sum(x => x.AllocatedAmount);

        var dto = new ShopSupplierPaymentDto
        {
            Id = entity.Id,
            PaymentNumber = entity.PaymentNumber,
            SupplierId = entity.SupplierId,
            SupplierCode = entity.Supplier?.Code ?? string.Empty,
            SupplierName = entity.Supplier?.Name ?? string.Empty,
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
                goodsReceipts.TryGetValue(x.GoodsReceiptId, out var gr);
                return new ShopSupplierPaymentAllocationDto
                {
                    Id = x.Id,
                    GoodsReceiptId = x.GoodsReceiptId,
                    GoodsReceiptNumber = gr?.GoodsReceiptNumber ?? string.Empty,
                    SupplierInvoiceNumber = gr?.SupplierInvoiceNumber,
                    ReceiptDate = gr?.ReceiptDate ?? default,
                    GrandTotal = gr?.GrandTotal,
                    AllocatedAmount = x.AllocatedAmount,
                    CreationTime = x.CreationTime
                };
            }).ToList()
        };
        await PopulateBankAccountNamesAsync(new List<ShopSupplierPaymentDto> { dto });
        return dto;
    }

    private async Task PopulateBankAccountNamesAsync(List<ShopSupplierPaymentDto> items)
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
