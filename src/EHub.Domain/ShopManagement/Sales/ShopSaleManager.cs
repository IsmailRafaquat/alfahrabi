using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.Units;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace EHub.ShopManagement.Sales;

public class ShopSaleManager : DomainService
{
    private const string DocumentType = "Sale";
    private const string NumberPrefix = "SAL-";

    private readonly IRepository<ShopSale, Guid> _repository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly IRepository<ShopProduct, Guid> _productRepository;
    private readonly IRepository<ShopUnit, Guid> _unitRepository;
    private readonly IRepository<ShopStockTransaction, Guid> _stockTransactionRepository;
    private readonly ShopDocumentNumberGenerator _numberGenerator;
    private readonly ShopCashRegisterManager _cashRegisterManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;

    public ShopSaleManager(
        IRepository<ShopSale, Guid> repository,
        IRepository<ShopCustomer, Guid> customerRepository,
        IRepository<ShopProduct, Guid> productRepository,
        IRepository<ShopUnit, Guid> unitRepository,
        IRepository<ShopStockTransaction, Guid> stockTransactionRepository,
        ShopDocumentNumberGenerator numberGenerator,
        ShopCashRegisterManager cashRegisterManager,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitRepository = unitRepository;
        _stockTransactionRepository = stockTransactionRepository;
        _numberGenerator = numberGenerator;
        _cashRegisterManager = cashRegisterManager;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
    }

    public async Task<ShopSale> CreateAsync(
        Guid customerId,
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        ShopSalePaymentMethod paymentMethod,
        decimal paidAmount,
        string? referenceNumber,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopSaleItemInput> items)
    {
        var tenantId = RequireTenant();
        var customer = await ValidateCustomerAsync(customerId, tenantId);

        var saleId = GuidGenerator.Create();
        var itemEntities = await BuildItemEntitiesAsync(saleId, tenantId, items);

        var saleNumber = await _numberGenerator.GetNextNumberAsync(tenantId, DocumentType, NumberPrefix);

        var sale = new ShopSale(saleId, tenantId, saleNumber, customerId, saleDate, dueDate, saleType, paymentMethod,
            referenceNumber, otherCharges, notes, paidAmount, itemEntities);

        await ValidateCreditLimitAsync(customer, saleType, sale.PendingAmount, null, tenantId);

        return sale;
    }

    public async Task UpdateAsync(
        ShopSale sale,
        Guid customerId,
        DateTime saleDate,
        DateTime? dueDate,
        ShopSaleType saleType,
        ShopSalePaymentMethod paymentMethod,
        decimal paidAmount,
        string? referenceNumber,
        decimal otherCharges,
        string? notes,
        IReadOnlyList<ShopSaleItemInput> items)
    {
        var tenantId = RequireTenantOwnership(sale);
        sale.EnsureEditable();

        var customer = await ValidateCustomerAsync(customerId, tenantId);
        var itemEntities = await BuildItemEntitiesAsync(sale.Id, tenantId, items);

        sale.UpdateHeaderAndItems(customerId, saleDate, dueDate, saleType, paymentMethod, referenceNumber,
            otherCharges, notes, paidAmount, itemEntities);

        await ValidateCreditLimitAsync(customer, saleType, sale.PendingAmount, sale.Id, tenantId);
    }

    public async Task CompleteAsync(ShopSale sale)
    {
        var tenantId = RequireTenantOwnership(sale);
        sale.EnsureCompletable();

        var customer = await ValidateCustomerAsync(sale.CustomerId, tenantId);
        await ValidateCreditLimitAsync(customer, sale.SaleType, sale.PendingAmount, sale.Id, tenantId);

        var productIds = sale.Items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var existingTransactionQuery = await _stockTransactionRepository.GetQueryableAsync();
        var itemIds = sale.Items.Select(x => x.Id).ToList();
        var alreadyHasTransactions = existingTransactionQuery.Any(x =>
            x.TenantId == tenantId && x.ReferenceType == ShopStockReferenceType.Sale && x.SourceItemId.HasValue && itemIds.Contains(x.SourceItemId.Value));
        if (alreadyHasTransactions) throw new BusinessException("ShopManagement:SaleStockTransactionAlreadyExists");

        var completedByUserId = _currentUser.GetId();
        var completedDate = Clock.Now;

        foreach (var item in sale.Items)
        {
            if (!products.TryGetValue(item.ProductId, out var product)) throw new BusinessException("ShopManagement:SaleProductNotFound");
            if (product.TrackBatch) throw new BusinessException("ShopManagement:SaleBatchTrackedProductNotSupported").WithData("Product", product.Name);
            if (product.TrackExpiry) throw new BusinessException("ShopManagement:SaleExpiryTrackedProductNotSupported").WithData("Product", product.Name);

            product.DecreaseStock(item.Quantity);
            item.SetCostSnapshot(product.PurchasePrice);

            var transaction = new ShopStockTransaction(
                GuidGenerator.Create(), tenantId, item.ProductId, ShopStockTransactionType.Sale, ShopStockReferenceType.Sale,
                sale.Id, sale.SaleNumber, item.Id, sale.SaleDate,
                0, item.Quantity, product.CurrentStock, item.UnitCostSnapshot, item.BatchNumber, item.ExpiryDate,
                null, completedByUserId, completedDate);
            await _stockTransactionRepository.InsertAsync(transaction, autoSave: true);
        }

        foreach (var product in products.Values)
        {
            await _productRepository.UpdateAsync(product, autoSave: true);
        }

        sale.MarkAsCompleted(completedByUserId, completedDate);

        // Only the cash actually received at completion time affects the cash drawer - never the
        // full GrandTotal, since a credit sale may only be partially paid in cash up front.
        if (sale.PaymentMethod == ShopSalePaymentMethod.Cash && sale.PaidAmount > 0)
        {
            await _cashRegisterManager.RecordAutomaticTransactionAsync(
                tenantId, ShopCashTransactionType.CashSale, ShopCashDirection.In, sale.PaidAmount,
                ShopCashReferenceType.Sale, sale.Id, sale.SaleNumber, $"Cash sale - {sale.SaleNumber}", sale.SaleDate);
        }
    }

    public Task CancelAsync(ShopSale sale, string cancellationReason)
    {
        RequireTenantOwnership(sale);
        sale.MarkAsCancelled(_currentUser.GetId(), Clock.Now, cancellationReason);
        return Task.CompletedTask;
    }

    public Task ValidateDeleteAsync(ShopSale sale)
    {
        RequireTenantOwnership(sale);
        sale.EnsureDeletable();
        return Task.CompletedTask;
    }

    private async Task<List<ShopSaleItem>> BuildItemEntitiesAsync(Guid saleId, Guid tenantId, IReadOnlyList<ShopSaleItemInput> items)
    {
        if (items == null || items.Count == 0) throw new BusinessException("ShopManagement:SaleRequiresItems");
        ValidateNoDuplicateProducts(items);

        var productIds = items.Select(x => x.ProductId).Distinct().ToList();
        var productQuery = await _productRepository.GetQueryableAsync();
        var products = productQuery.Where(x => productIds.Contains(x.Id) && x.TenantId == tenantId).ToList().ToDictionary(x => x.Id);

        var unitIds = products.Values.Select(x => x.UnitId).Distinct().ToList();
        var unitQuery = await _unitRepository.GetQueryableAsync();
        var units = unitQuery.Where(x => unitIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var itemEntities = new List<ShopSaleItem>();
        foreach (var input in items)
        {
            if (!products.TryGetValue(input.ProductId, out var product)) throw new BusinessException("ShopManagement:SaleProductNotFound");
            if (!product.IsActive) throw new BusinessException("ShopManagement:SaleProductInactive").WithData("Product", product.Name);
            if (!units.TryGetValue(product.UnitId, out var unit)) throw new BusinessException("ShopManagement:SaleProductNotFound");

            if (input.Quantity <= 0) throw new BusinessException("ShopManagement:SaleInvalidQuantity");
            if (!unit.AllowDecimal && input.Quantity != Math.Truncate(input.Quantity))
                throw new BusinessException("ShopManagement:SaleWholeQuantityRequired");

            itemEntities.Add(new ShopSaleItem(
                GuidGenerator.Create(), tenantId, saleId, product.Id,
                product.Code, product.Name, unit.Name, unit.ShortName,
                input.Quantity, input.UnitSalePrice, input.DiscountPercentage, input.TaxPercentage,
                input.BatchNumber, input.ExpiryDate));
        }

        return itemEntities;
    }

    private static void ValidateNoDuplicateProducts(IReadOnlyList<ShopSaleItemInput> items)
    {
        var ids = items.Select(x => x.ProductId).ToList();
        if (ids.Distinct().Count() != ids.Count) throw new BusinessException("ShopManagement:SaleDuplicateProduct");
    }

    private async Task<ShopCustomer> ValidateCustomerAsync(Guid customerId, Guid tenantId)
    {
        var query = await _customerRepository.GetQueryableAsync();
        var customer = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == customerId && x.TenantId == tenantId));
        if (customer == null) throw new BusinessException("ShopManagement:SaleCustomerNotFound");
        if (!customer.IsActive) throw new BusinessException("ShopManagement:SaleCustomerInactive").WithData("Customer", customer.Name);
        return customer;
    }

    private async Task ValidateCreditLimitAsync(ShopCustomer customer, ShopSaleType saleType, decimal pendingAmount, Guid? excludeSaleId, Guid tenantId)
    {
        if (saleType != ShopSaleType.Credit) return;
        if (customer.CreditLimit <= 0) return;
        if (pendingAmount <= 0) return;

        var query = await _repository.GetQueryableAsync();
        var otherCompletedPending = await AsyncExecuter.SumAsync(query.Where(x =>
            x.TenantId == tenantId && x.CustomerId == customer.Id && x.Status == ShopSaleStatus.Completed &&
            (!excludeSaleId.HasValue || x.Id != excludeSaleId.Value)).Select(x => x.PendingAmount));

        var projectedBalance = customer.OpeningBalance + otherCompletedPending + pendingAmount;
        if (projectedBalance > customer.CreditLimit)
            throw new BusinessException("ShopManagement:SaleExceedsCustomerCreditLimit").WithData("Customer", customer.Name);
    }

    private Guid RequireTenantOwnership(ShopSale sale)
    {
        var tenantId = RequireTenant();
        if (sale.TenantId != tenantId) throw new BusinessException("ShopManagement:SaleNotFound");
        return tenantId;
    }

    private Guid RequireTenant() => _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
}
