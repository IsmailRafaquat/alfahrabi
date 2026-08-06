using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Products;

public class ShopProductLookupDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }
    public decimal SalePrice { get; set; }
    public decimal CurrentStock { get; set; }
    public bool IsActive { get; set; }
}
