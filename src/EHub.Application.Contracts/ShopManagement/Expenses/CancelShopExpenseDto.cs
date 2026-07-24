using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Expenses;

public class CancelShopExpenseDto
{
    [Required]
    public string CancellationReason { get; set; } = string.Empty;
}
