using Volo.Abp.Application.Dtos;
namespace EHub.ShopManagement.Units;
public class GetShopUnitsInput : PagedAndSortedResultRequestDto { public string? Filter { get; set; } public bool? AllowDecimal { get; set; } public bool? IsActive { get; set; } }
