using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Sales;

public class ShopSaleDto : EntityDto<Guid>
{
    public string SaleNumber { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public DateTime SaleDate { get; set; }
    public DateTime? DueDate { get; set; }
    public ShopSaleType SaleType { get; set; }
    public ShopSalePaymentMethod PaymentMethod { get; set; }
    public ShopSaleStatus Status { get; set; }

    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? PendingAmount { get; set; }

    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    public Guid? CompletedByUserId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ShopSaleItemDto> Items { get; set; } = new();
}
