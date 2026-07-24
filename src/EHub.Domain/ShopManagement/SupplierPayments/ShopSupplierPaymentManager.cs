using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Suppliers;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.SupplierPayments;

public class ShopSupplierPaymentManager : DomainService
{
    private const string DocumentType = "SupplierPayment";
    private const string NumberPrefix = "SP-";

    private readonly IRepository<ShopSupplierPayment, Guid> _repository;
    private readonly IRepository<ShopSupplier, Guid> _supplierRepository;
    private readonly IRepository<ShopGoodsReceipt, Guid> _goodsReceiptRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ShopCashRegisterManager _cashRegisterManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopSupplierPaymentManager(
        IRepository<ShopSupplierPayment, Guid> repository,
        IRepository<ShopSupplier, Guid> supplierRepository,
        IRepository<ShopGoodsReceipt, Guid> goodsReceiptRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ShopCashRegisterManager cashRegisterManager,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _numberGenerator = numberGenerator;
        _cashRegisterManager = cashRegisterManager;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopSupplierPayment> CreateAsync(
        Guid supplierId,
        DateTime paymentDate,
        ShopSupplierPaymentType paymentType,
        ShopSupplierPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        IReadOnlyList<ShopSupplierPaymentAllocationInput> allocations)
    {
        var tenantId = RequireTenant();
        await ValidateSupplierAsync(supplierId, tenantId);

        var paymentId = GuidGenerator.Create();
        var allocationEntities = await BuildAllocationEntitiesAsync(paymentId, tenantId, supplierId, allocations, excludePaymentId: null);

        var paymentNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopSupplierPayment(paymentId, tenantId, paymentNumber, supplierId, paymentDate, paymentType,
            paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes, allocationEntities);
    }

    public async Task UpdateAsync(
        ShopSupplierPayment payment,
        DateTime paymentDate,
        ShopSupplierPaymentType paymentType,
        ShopSupplierPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        IReadOnlyList<ShopSupplierPaymentAllocationInput> allocations)
    {
        var tenantId = RequireTenantOwnership(payment);
        payment.EnsureEditable();

        var allocationEntities = await BuildAllocationEntitiesAsync(payment.Id, tenantId, payment.SupplierId, allocations, excludePaymentId: payment.Id);

        payment.Update(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes, allocationEntities);
    }

    public async Task PostAsync(ShopSupplierPayment payment)
    {
        var tenantId = RequireTenantOwnership(payment);

        // Revalidate pending amounts at posting time, excluding this payment's own (not-yet-posted) allocations.
        var inputs = payment.Allocations.Select(x => new ShopSupplierPaymentAllocationInput { GoodsReceiptId = x.GoodsReceiptId, AllocatedAmount = x.AllocatedAmount }).ToList();
        await ValidateAllocationsAgainstPendingAsync(tenantId, payment.SupplierId, inputs, excludePaymentId: payment.Id);

        payment.MarkAsPosted(_currentUser.GetId(), Clock.Now);

        if (payment.PaymentMethod == ShopSupplierPaymentMethod.Cash)
        {
            await _cashRegisterManager.RecordAutomaticTransactionAsync(
                tenantId, ShopCashTransactionType.SupplierPayment, ShopCashDirection.Out, payment.Amount,
                ShopCashReferenceType.SupplierPayment, payment.Id, payment.PaymentNumber, $"Supplier payment - {payment.PaymentNumber}", payment.PaymentDate);
        }
    }

    public async Task CancelAsync(ShopSupplierPayment payment, string cancellationReason)
    {
        var tenantId = RequireTenantOwnership(payment);
        payment.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);

        if (payment.PaymentMethod == ShopSupplierPaymentMethod.Cash)
        {
            await _cashRegisterManager.RecordReversalIfExistsAsync(
                tenantId, ShopCashTransactionType.SupplierPayment, ShopCashReferenceType.SupplierPayment,
                payment.Id, payment.PaymentNumber, $"Reversal - cancelled supplier payment {payment.PaymentNumber}", Clock.Now);
        }
    }

    public Task ValidateDeleteAsync(ShopSupplierPayment payment)
    {
        RequireTenantOwnership(payment);
        payment.EnsureDeletable();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Computes, for the given Goods Receipts, the amount already covered by Posted payment
    /// allocations (optionally excluding one payment, e.g. the one currently being edited/posted).
    /// </summary>
    public async Task<Dictionary<Guid, decimal>> GetPostedAllocatedAmountsAsync(Guid tenantId, IReadOnlyList<Guid> goodsReceiptIds, Guid? excludePaymentId)
    {
        if (goodsReceiptIds.Count == 0) return new Dictionary<Guid, decimal>();

        var query = await _repository.GetQueryableAsync();
        var grouped = query
            .Where(p => p.TenantId == tenantId && p.Status == ShopSupplierPaymentStatus.Posted && (!excludePaymentId.HasValue || p.Id != excludePaymentId.Value))
            .SelectMany(p => p.Allocations)
            .Where(a => goodsReceiptIds.Contains(a.GoodsReceiptId))
            .GroupBy(a => a.GoodsReceiptId)
            .Select(g => new { GoodsReceiptId = g.Key, Allocated = g.Sum(a => a.AllocatedAmount) });

        var result = await AsyncExecuter.ToListAsync(grouped);
        return result.ToDictionary(x => x.GoodsReceiptId, x => x.Allocated);
    }

    private async Task<List<ShopSupplierPaymentAllocation>> BuildAllocationEntitiesAsync(
        Guid paymentId,
        Guid tenantId,
        Guid supplierId,
        IReadOnlyList<ShopSupplierPaymentAllocationInput> allocations,
        Guid? excludePaymentId)
    {
        allocations ??= new List<ShopSupplierPaymentAllocationInput>();
        if (allocations.Count == 0) return new List<ShopSupplierPaymentAllocation>();

        ValidateNoDuplicateGoodsReceipts(allocations);

        var goodsReceiptIds = allocations.Select(x => x.GoodsReceiptId).Distinct().ToList();
        var grQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var goodsReceipts = grQuery.Where(x => goodsReceiptIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        foreach (var input in allocations)
        {
            if (input.AllocatedAmount <= 0) throw new BusinessException("ShopManagement:SupplierPaymentInvalidAllocationAmount");
            if (!goodsReceipts.TryGetValue(input.GoodsReceiptId, out var gr)) throw new BusinessException("ShopManagement:SupplierPaymentGoodsReceiptNotFound");
            if (gr.SupplierId != supplierId) throw new BusinessException("ShopManagement:SupplierPaymentGoodsReceiptSupplierMismatch");
            if (gr.Status != ShopGoodsReceiptStatus.Completed) throw new BusinessException("ShopManagement:SupplierPaymentGoodsReceiptNotCompleted");
        }

        await ValidateAllocationsAgainstPendingAsync(tenantId, supplierId, allocations, excludePaymentId);

        return allocations.Select(input => new ShopSupplierPaymentAllocation(GuidGenerator.Create(), tenantId, paymentId, input.GoodsReceiptId, input.AllocatedAmount)).ToList();
    }

    private async Task ValidateAllocationsAgainstPendingAsync(Guid tenantId, Guid supplierId, IReadOnlyList<ShopSupplierPaymentAllocationInput> allocations, Guid? excludePaymentId)
    {
        if (allocations.Count == 0) return;

        var goodsReceiptIds = allocations.Select(x => x.GoodsReceiptId).Distinct().ToList();
        var grQuery = await _goodsReceiptRepository.GetQueryableAsync();
        var goodsReceipts = grQuery.Where(x => goodsReceiptIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);
        var postedAllocated = await GetPostedAllocatedAmountsAsync(tenantId, goodsReceiptIds, excludePaymentId);

        foreach (var input in allocations)
        {
            if (!goodsReceipts.TryGetValue(input.GoodsReceiptId, out var gr)) throw new BusinessException("ShopManagement:SupplierPaymentGoodsReceiptNotFound");
            var alreadyAllocated = postedAllocated.TryGetValue(input.GoodsReceiptId, out var value) ? value : 0;
            var pending = (gr.GrandTotal) - alreadyAllocated;
            if (input.AllocatedAmount > pending) throw new BusinessException("ShopManagement:SupplierPaymentAllocationExceedsPending");
        }
    }

    private static void ValidateNoDuplicateGoodsReceipts(IReadOnlyList<ShopSupplierPaymentAllocationInput> allocations)
    {
        var ids = allocations.Select(x => x.GoodsReceiptId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:SupplierPaymentDuplicateGoodsReceiptAllocation");
    }

    private async Task ValidateSupplierAsync(Guid supplierId, Guid tenantId)
    {
        var query = await _supplierRepository.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(query.Where(x => x.Id == supplierId && x.TenantId == tenantId));
        if (!exists) throw new BusinessException("ShopManagement:SupplierPaymentSupplierNotFound");
    }

    private Guid RequireTenantOwnership(ShopSupplierPayment payment)
    {
        var tenantId = RequireTenant();
        if (payment.TenantId != tenantId) throw new BusinessException("ShopManagement:SupplierPaymentNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
