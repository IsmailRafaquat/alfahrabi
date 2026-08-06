using System;

namespace EHub.ShopManagement.SupplierPayments;

/// <summary>
/// Domain-layer input for a supplier payment allocation, decoupled from the application layer's
/// DTOs so that <see cref="ShopSupplierPaymentManager"/> does not depend on EHub.Application.Contracts.
/// </summary>
public class ShopSupplierPaymentAllocationInput
{
    public Guid GoodsReceiptId { get; set; }
    public decimal AllocatedAmount { get; set; }
}
