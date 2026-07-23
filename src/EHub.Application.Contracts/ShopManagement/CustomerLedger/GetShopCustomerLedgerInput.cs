using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CustomerLedger;

public class GetShopCustomerLedgerInput
{
    [Required]
    public Guid CustomerId { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public ShopCustomerLedgerReferenceType? ReferenceType { get; set; }
    public string? Filter { get; set; }
}
