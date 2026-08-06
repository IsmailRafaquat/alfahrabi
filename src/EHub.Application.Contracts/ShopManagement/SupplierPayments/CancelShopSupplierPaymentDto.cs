using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.SupplierPayments;

public class CancelShopSupplierPaymentDto
{
    [Required, StringLength(ShopSupplierPaymentConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}
