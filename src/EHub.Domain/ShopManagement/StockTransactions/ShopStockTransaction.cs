using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Products;

namespace EHub.ShopManagement.StockTransactions;

/// <summary>
/// An immutable stock ledger entry. No update or delete is ever exposed for this entity.
/// </summary>
public class ShopStockTransaction : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid ProductId { get; protected set; }
    public ShopProduct? Product { get; protected set; }

    public ShopStockTransactionType TransactionType { get; protected set; }
    public ShopStockReferenceType ReferenceType { get; protected set; }
    public Guid ReferenceId { get; protected set; }
    public string ReferenceNumber { get; protected set; } = string.Empty;

    /// <summary>
    /// The source line item (e.g. a ShopGoodsReceiptItem.Id) that produced this transaction.
    /// Combined with a unique index on TenantId + ReferenceType + SourceItemId, this guarantees
    /// each source item can only ever create one stock transaction (idempotency).
    /// </summary>
    public Guid? SourceItemId { get; protected set; }

    public DateTime TransactionDate { get; protected set; }
    public decimal QuantityIn { get; protected set; }
    public decimal QuantityOut { get; protected set; }
    public decimal BalanceQuantity { get; protected set; }
    public decimal UnitCost { get; protected set; }
    public decimal TotalCost { get; protected set; }
    public string? BatchNumber { get; protected set; }
    public DateTime? ExpiryDate { get; protected set; }
    public string? Notes { get; protected set; }

    public Guid? CreatedByUserId { get; protected set; }
    public DateTime CreationTime { get; protected set; }

    protected ShopStockTransaction() { }

    internal ShopStockTransaction(
        Guid id,
        Guid tenantId,
        Guid productId,
        ShopStockTransactionType transactionType,
        ShopStockReferenceType referenceType,
        Guid referenceId,
        string referenceNumber,
        Guid? sourceItemId,
        DateTime transactionDate,
        decimal quantityIn,
        decimal quantityOut,
        decimal balanceQuantity,
        decimal unitCost,
        string? batchNumber,
        DateTime? expiryDate,
        string? notes,
        Guid? createdByUserId,
        DateTime creationTime) : base(id)
    {
        TenantId = tenantId;
        ProductId = productId;
        TransactionType = transactionType;
        ReferenceType = referenceType;
        ReferenceId = referenceId;
        ReferenceNumber = Check.NotNullOrWhiteSpace(referenceNumber, nameof(referenceNumber), ShopStockTransactionConsts.ReferenceNumberMaxLength);
        SourceItemId = sourceItemId;
        TransactionDate = transactionDate;
        QuantityIn = quantityIn;
        QuantityOut = quantityOut;
        BalanceQuantity = balanceQuantity;
        UnitCost = unitCost;
        TotalCost = Math.Round((quantityIn + quantityOut) * unitCost, 2, MidpointRounding.AwayFromZero);
        BatchNumber = Check.Length(batchNumber?.Trim(), nameof(batchNumber), ShopStockTransactionConsts.BatchNumberMaxLength);
        ExpiryDate = expiryDate;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopStockTransactionConsts.NotesMaxLength);
        CreatedByUserId = createdByUserId;
        CreationTime = creationTime;
    }
}
