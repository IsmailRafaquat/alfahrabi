using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SaleReturns;

public class UpdateShopSaleReturnItemDto : CreateShopSaleReturnItemDto
{
}

public class UpdateShopSaleReturnDto
{
    public DateTime ReturnDate { get; set; }
    public ShopSaleReturnReason Reason { get; set; }
    public string? ReasonDetails { get; set; }
    public ShopSaleReturnSettlementType SettlementType { get; set; }
    public Guid? BankAccountId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal OtherCharges { get; set; }

    public string? Notes { get; set; }

    [MinLength(1)]
    public List<UpdateShopSaleReturnItemDto> Items { get; set; } = new();
}
