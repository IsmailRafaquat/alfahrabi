using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Sales;

public class ShopSaleProductLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }

    public decimal CurrentStock { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal TaxPercentage { get; set; }

    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
}
