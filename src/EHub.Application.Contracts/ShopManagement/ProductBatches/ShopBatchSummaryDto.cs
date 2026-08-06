using System;

namespace EHub.ShopManagement.ProductBatches;

public class ShopBatchSummaryDto
{
    public Guid ProductId { get; set; }
    public int TotalBatches { get; set; }
    public int ActiveBatches { get; set; }
    public int NearExpiryBatches { get; set; }
    public int ExpiredBatches { get; set; }
    public int ExhaustedBatches { get; set; }
    public int BlockedBatches { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal TotalIssuedQuantity { get; set; }
    public decimal TotalAvailableQuantity { get; set; }
    public DateTime? NearestExpiryDate { get; set; }
}
