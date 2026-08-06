using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Dashboard;

public class GetShopDashboardInput
{
    public ShopDashboardPeriod Period { get; set; } = ShopDashboardPeriod.Today;

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    [Range(1, 20)]
    public int TopProductsCount { get; set; } = 5;

    [Range(1, 20)]
    public int RecentItemsCount { get; set; } = 5;
}
