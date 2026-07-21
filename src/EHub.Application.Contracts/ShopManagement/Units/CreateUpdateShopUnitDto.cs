using System.ComponentModel.DataAnnotations;
namespace EHub.ShopManagement.Units;
public class CreateUpdateShopUnitDto { [Required, StringLength(ShopUnitConsts.NameMaxLength)] public string Name { get; set; } = string.Empty; [Required, StringLength(ShopUnitConsts.ShortNameMaxLength)] public string ShortName { get; set; } = string.Empty; public bool AllowDecimal { get; set; } public bool IsActive { get; set; } = true; }
