using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SaleReturns;

public class CreateShopSaleReturnItemDto
{
    [Required]
    public Guid SaleItemId { get; set; }

    [Range(typeof(decimal), "0.0001", "999999999999")]
    public decimal ReturnQuantity { get; set; }

    public ShopSaleReturnReason Reason { get; set; }
    public string? Notes { get; set; }
}

public class CreateShopSaleReturnDto
{
    [Required]
    public Guid SaleId { get; set; }

    public DateTime ReturnDate { get; set; }
    public ShopSaleReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public ShopSaleReturnSettlementType SettlementType { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OtherCharges { get; set; }

    public string? Notes { get; set; }

    [MinLength(1)]
    public List<CreateShopSaleReturnItemDto> Items { get; set; } = new();
}
