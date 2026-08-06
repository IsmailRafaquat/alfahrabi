using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturnItemDto : EntityDto<Guid>
{
    public Guid SaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;

    public decimal SoldQuantity { get; set; }
    public decimal PreviouslyReturnedQuantity { get; set; }
    public decimal ReturnableQuantity { get; set; }
    public decimal ReturnQuantity { get; set; }

    public decimal? UnitSalePrice { get; set; }
    public decimal? UnitCostSnapshot { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal TaxPercentage { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? LineSubTotal { get; set; }
    public decimal? LineTotal { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public ShopSaleReturnReason Reason { get; set; }
    public string? Notes { get; set; }
}

public class ShopSaleReturnDto : EntityDto<Guid>
{
    public string SaleReturnNumber { get; set; } = string.Empty;

    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime ReturnDate { get; set; }
    public ShopSaleReturnStatus Status { get; set; }
    public ShopSaleReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public ShopSaleReturnSettlementType SettlementType { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountCode { get; set; }
    public string? BankAccountName { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? RefundAmount { get; set; }
    public decimal? CustomerCreditAmount { get; set; }

    public string? Notes { get; set; }

    public DateTime? CompletedDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopSaleReturnItemDto> Items { get; set; } = new();
}
