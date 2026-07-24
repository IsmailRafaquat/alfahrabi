using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturnableItemDto : EntityDto<Guid>
{
    public Guid SaleItemId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public decimal SoldQuantity { get; set; }
    public decimal PreviouslyReturnedQuantity { get; set; }
    public decimal ReturnableQuantity { get; set; }

    public decimal? UnitSalePrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TaxPercentage { get; set; }

    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class ShopSaleReturnableDto : EntityDto<Guid>
{
    public string SaleNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? PendingAmount { get; set; }

    public List<ShopSaleReturnableItemDto> Items { get; set; } = new();
}
