using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SupplierPayments;

public class CreateUpdateShopSupplierPaymentAllocationDto
{
    [Required]
    public Guid GoodsReceiptId { get; set; }

    // Business validation (must be > 0 and not exceed the receipt's pending amount) is
    // enforced by ShopSupplierPaymentManager as a localized BusinessException.
    public decimal AllocatedAmount { get; set; }
}
