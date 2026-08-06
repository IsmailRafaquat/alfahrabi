using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.StockAdjustments;

public class ShopStockAdjustmentProductLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string UnitShortName { get; set; } = string.Empty;
    public bool UnitAllowDecimal { get; set; }
    public decimal CurrentStock { get; set; }
    public bool TrackBatch { get; set; }
    public bool TrackExpiry { get; set; }
    public bool TrackSerialNumber { get; set; }
}
