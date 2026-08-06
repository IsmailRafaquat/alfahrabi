using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockTransactions;

public class ShopStockTransactionDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;

    public ShopStockTransactionType TransactionType { get; set; }
    public ShopStockReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }
    public decimal QuantityIn { get; set; }
    public decimal QuantityOut { get; set; }
    public decimal BalanceQuantity { get; set; }
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? ProductBatchId { get; set; }
    public decimal? BatchBalanceQuantity { get; set; }
    public string? Notes { get; set; }

    public DateTime CreationTime { get; set; }
}
