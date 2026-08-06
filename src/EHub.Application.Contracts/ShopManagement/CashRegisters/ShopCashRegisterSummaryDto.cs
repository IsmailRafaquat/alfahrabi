namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegisterSummaryDto
{
    public decimal? OpeningCash { get; set; }
    public decimal? CashSales { get; set; }
    public decimal? CustomerCashPayments { get; set; }
    public decimal? SupplierCashPayments { get; set; }
    public decimal? CashExpenses { get; set; }
    public decimal? CustomerRefunds { get; set; }
    public decimal? ManualCashIn { get; set; }
    public decimal? ManualCashOut { get; set; }
    public decimal? ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal? DifferenceAmount { get; set; }
    public bool IsShort { get; set; }
    public bool IsExcess { get; set; }
}
