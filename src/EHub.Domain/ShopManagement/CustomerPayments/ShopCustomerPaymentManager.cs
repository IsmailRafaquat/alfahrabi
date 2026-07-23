using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPaymentManager : DomainService
{
    private const string DocumentType = "CustomerPayment";
    private const string NumberPrefix = "CP-";

    private readonly IRepository<ShopCustomerPayment, Guid> _repository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopSale, Guid> _saleRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopCustomerPaymentManager(
        IRepository<ShopCustomerPayment, Guid> repository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopSale, Guid> saleRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _saleRepository = saleRepository;
        _numberGenerator = numberGenerator;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopCustomerPayment> CreateAsync(
        Guid customerId,
        DateTime paymentDate,
        ShopCustomerPaymentType paymentType,
        ShopCustomerPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        IReadOnlyList<ShopCustomerPaymentAllocationInput> allocations)
    {
        var tenantId = RequireTenant();
        await ValidateCustomerAsync(customerId, tenantId);

        var paymentId = GuidGenerator.Create();
        var allocationEntities = await BuildAllocationEntitiesAsync(paymentId, tenantId, customerId, allocations, excludePaymentId: null);

        var paymentNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        return new ShopCustomerPayment(paymentId, tenantId, paymentNumber, customerId, paymentDate, paymentType,
            paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes, allocationEntities);
    }

    public async Task UpdateAsync(
        ShopCustomerPayment payment,
        DateTime paymentDate,
        ShopCustomerPaymentType paymentType,
        ShopCustomerPaymentMethod paymentMethod,
        decimal amount,
        string? referenceNumber,
        string? chequeNumber,
        string? bankName,
        string? notes,
        IReadOnlyList<ShopCustomerPaymentAllocationInput> allocations)
    {
        var tenantId = RequireTenantOwnership(payment);
        payment.EnsureEditable();

        var allocationEntities = await BuildAllocationEntitiesAsync(payment.Id, tenantId, payment.CustomerId, allocations, excludePaymentId: payment.Id);

        payment.Update(paymentDate, paymentType, paymentMethod, amount, referenceNumber, chequeNumber, bankName, notes, allocationEntities);
    }

    public async Task PostAsync(ShopCustomerPayment payment)
    {
        var tenantId = RequireTenantOwnership(payment);

        // Revalidate pending amounts at posting time, excluding this payment's own (not-yet-posted) allocations.
        var inputs = payment.Allocations.Select(x => new ShopCustomerPaymentAllocationInput { SaleId = x.SaleId, AllocatedAmount = x.AllocatedAmount }).ToList();
        await ValidateAllocationsAgainstPendingAsync(tenantId, payment.CustomerId, inputs, excludePaymentId: payment.Id);

        payment.MarkAsPosted(_currentUser.GetId(), Clock.Now);
    }

    public Task CancelAsync(ShopCustomerPayment payment, string cancellationReason)
    {
        RequireTenantOwnership(payment);
        payment.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopCustomerPayment payment)
    {
        RequireTenantOwnership(payment);
        payment.EnsureDeletable();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Computes, for the given Sales, the amount already covered by Posted payment
    /// allocations (optionally excluding one payment, e.g. the one currently being edited/posted).
    /// </summary>
    public async Task<Dictionary<Guid, decimal>> GetPostedAllocatedAmountsAsync(Guid tenantId, IReadOnlyList<Guid> saleIds, Guid? excludePaymentId)
    {
        if (saleIds.Count == 0) return new Dictionary<Guid, decimal>();

        var query = await _repository.GetQueryableAsync();
        var grouped = query
            .Where(p => p.TenantId == tenantId && p.Status == ShopCustomerPaymentStatus.Posted && (!excludePaymentId.HasValue || p.Id != excludePaymentId.Value))
            .SelectMany(p => p.Allocations)
            .Where(a => saleIds.Contains(a.SaleId))
            .GroupBy(a => a.SaleId)
            .Select(g => new { SaleId = g.Key, Allocated = g.Sum(a => a.AllocatedAmount) });

        var result = await AsyncExecuter.ToListAsync(grouped);
        return result.ToDictionary(x => x.SaleId, x => x.Allocated);
    }

    private async Task<List<ShopCustomerPaymentAllocation>> BuildAllocationEntitiesAsync(
        Guid paymentId,
        Guid tenantId,
        Guid customerId,
        IReadOnlyList<ShopCustomerPaymentAllocationInput> allocations,
        Guid? excludePaymentId)
    {
        allocations ??= new List<ShopCustomerPaymentAllocationInput>();
        if (allocations.Count == 0) return new List<ShopCustomerPaymentAllocation>();

        ValidateNoDuplicateSales(allocations);

        var saleIds = allocations.Select(x => x.SaleId).Distinct().ToList();
        var saleQuery = await _saleRepository.GetQueryableAsync();
        var sales = saleQuery.Where(x => saleIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        foreach (var input in allocations)
        {
            if (input.AllocatedAmount <= 0) throw new BusinessException("ShopManagement:CustomerPaymentInvalidAllocationAmount");
            if (!sales.TryGetValue(input.SaleId, out var sale)) throw new BusinessException("ShopManagement:CustomerPaymentSaleNotFound");
            if (sale.CustomerId != customerId) throw new BusinessException("ShopManagement:CustomerPaymentSaleCustomerMismatch");
            if (sale.Status != ShopSaleStatus.Completed) throw new BusinessException("ShopManagement:CustomerPaymentSaleNotCompleted");
        }

        await ValidateAllocationsAgainstPendingAsync(tenantId, customerId, allocations, excludePaymentId);

        return allocations.Select(input => new ShopCustomerPaymentAllocation(GuidGenerator.Create(), tenantId, paymentId, input.SaleId, input.AllocatedAmount)).ToList();
    }

    private async Task ValidateAllocationsAgainstPendingAsync(Guid tenantId, Guid customerId, IReadOnlyList<ShopCustomerPaymentAllocationInput> allocations, Guid? excludePaymentId)
    {
        if (allocations.Count == 0) return;

        var saleIds = allocations.Select(x => x.SaleId).Distinct().ToList();
        var saleQuery = await _saleRepository.GetQueryableAsync();
        var sales = saleQuery.Where(x => saleIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);
        var postedAllocated = await GetPostedAllocatedAmountsAsync(tenantId, saleIds, excludePaymentId);

        foreach (var input in allocations)
        {
            if (!sales.TryGetValue(input.SaleId, out var sale)) throw new BusinessException("ShopManagement:CustomerPaymentSaleNotFound");
            var alreadyAllocated = postedAllocated.TryGetValue(input.SaleId, out var value) ? value : 0;
            var pending = Math.Max(0, sale.GrandTotal - sale.PaidAmount - alreadyAllocated);
            if (input.AllocatedAmount > pending) throw new BusinessException("ShopManagement:CustomerPaymentAllocationExceedsPending");
        }
    }

    private static void ValidateNoDuplicateSales(IReadOnlyList<ShopCustomerPaymentAllocationInput> allocations)
    {
        var ids = allocations.Select(x => x.SaleId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:CustomerPaymentDuplicateSaleAllocation");
    }

    private async Task ValidateCustomerAsync(Guid customerId, Guid tenantId)
    {
        var query = await _customerRepository.GetQueryableAsync();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == customerId && x.TenantId == tenantId));
        if (customer == null) throw new BusinessException("ShopManagement:CustomerPaymentCustomerNotFound");
        if (!customer.IsActive) throw new BusinessException("ShopManagement:CustomerPaymentCustomerInactive").WithData("Customer", customer.Name);
    }

    private Guid RequireTenantOwnership(ShopCustomerPayment payment)
    {
        var tenantId = RequireTenant();
        if (payment.TenantId != tenantId) throw new BusinessException("ShopManagement:CustomerPaymentNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
