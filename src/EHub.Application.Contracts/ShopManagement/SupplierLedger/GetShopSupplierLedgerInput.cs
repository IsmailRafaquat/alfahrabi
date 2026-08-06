using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SupplierLedger;

public class GetShopSupplierLedgerInput
{
    [Required]
    public Guid SupplierId { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ShopSupplierLedgerReferenceType? ReferenceType { get; set; }
    public string? Filter { get; set; }
}
