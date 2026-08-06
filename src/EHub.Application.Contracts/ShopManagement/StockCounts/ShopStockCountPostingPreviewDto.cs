using System.Collections.Generic;

namespace EHub.ShopManagement.StockCounts;

public class ShopStockCountPostingPreviewDto
{
    public int TotalProducts { get; set; }
    public int NoDifferenceCount { get; set; }
    public int IncreaseCount { get; set; }
    public int DecreaseCount { get; set; }
    public decimal TotalIncreaseQuantity { get; set; }
    public decimal TotalDecreaseQuantity { get; set; }
    public bool CanPost { get; set; }
    public List<string> ChangedProducts { get; set; } = new();
}
