using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SupplierPayments;

public class CreateUpdateShopSupplierPaymentDto
{
    [Required]
    public Guid SupplierId { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    public ShopSupplierPaymentType PaymentType { get; set; }

    [Required]
    public ShopSupplierPaymentMethod PaymentMethod { get; set; }

    // Must be > 0; enforced by ShopSupplierPayment as a localized BusinessException.
    public decimal Amount { get; set; }

    [StringLength(ShopSupplierPaymentConsts.ReferenceNumberMaxLength)]
    public string? ReferenceNumber { get; set; }

    [StringLength(ShopSupplierPaymentConsts.ChequeNumberMaxLength)]
    public string? ChequeNumber { get; set; }

    [StringLength(ShopSupplierPaymentConsts.BankNameMaxLength)]
    public string? BankName { get; set; }

    [StringLength(ShopSupplierPaymentConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    public List<CreateUpdateShopSupplierPaymentAllocationDto> Allocations { get; set; } = new();
}
