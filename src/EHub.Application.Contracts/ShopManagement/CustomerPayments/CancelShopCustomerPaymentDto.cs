using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CustomerPayments;

public class CancelShopCustomerPaymentDto
{
    [Required, StringLength(ShopCustomerPaymentConsts.CancellationReasonMaxLength)]
    public string CancellationReason { get; set; } = string.Empty;
}
