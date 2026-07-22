using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.SupplierPayments;

public class ShopSupplierPaymentAllocationDto : EntityDto<Guid>
{
    public Guid GoodsReceiptId { get; set; }
    public string GoodsReceiptNumber { get; set; } = string.Empty;
    public string? SupplierInvoiceNumber { get; set; }
    public DateTime ReceiptDate { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? AllocatedAmount { get; set; }
    public DateTime CreationTime { get; set; }
}
