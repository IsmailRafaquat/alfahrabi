using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.CashRegisters;

public class CreateUpdateShopCashRegisterDto
{
    [Required, StringLength(ShopCashRegisterConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(ShopCashRegisterConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ShopCashRegisterConsts.DescriptionMaxLength)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
