namespace EHub.ShopManagement.Dashboard;

public class ShopDashboardBalanceSummaryDto
{
    public decimal CashBalance { get; set; }
    public decimal BankBalance { get; set; }
    public decimal CustomerReceivables { get; set; }
    public decimal SupplierPayables { get; set; }
    public decimal CustomerAdvanceBalance { get; set; }
    public decimal SupplierAdvanceBalance { get; set; }
}
