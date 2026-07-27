using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.ProductBatches;

public class ShopProductBatchLookupDto : EntityDto<Guid>
{
    public Guid ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public int? DaysToExpiry { get; set; }
    public decimal AvailableQuantity { get; set; }
    public ShopProductBatchStatus Status { get; set; }
    public bool IsBlocked { get; set; }
}
