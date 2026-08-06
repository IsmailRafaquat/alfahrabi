namespace EHub.ShopManagement.BankAccounts;

public class ShopBankAccountSummaryDto
{
    public decimal? OpeningBalance { get; set; }
    public decimal? TotalMoneyIn { get; set; }
    public decimal? TotalMoneyOut { get; set; }
    public decimal? CurrentBalance { get; set; }

    public decimal? CustomerPayments { get; set; }
    public decimal? SupplierPayments { get; set; }
    public decimal? Expenses { get; set; }
    public decimal? CustomerRefunds { get; set; }
    public decimal? CashDeposits { get; set; }
    public decimal? CashWithdrawals { get; set; }
    public decimal? BankTransfersIn { get; set; }
    public decimal? BankTransfersOut { get; set; }
    public decimal? ManualDeposits { get; set; }
    public decimal? ManualWithdrawals { get; set; }
}
