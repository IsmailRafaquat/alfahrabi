namespace EHub.ShopManagement.BankAccounts;

public enum ShopBankTransactionType
{
    OpeningBalance = 0,
    CustomerPayment = 1,
    SupplierPayment = 2,
    Expense = 3,
    CustomerRefund = 4,
    CashDeposit = 5,
    CashWithdrawal = 6,
    BankTransferIn = 7,
    BankTransferOut = 8,
    ManualDeposit = 9,
    ManualWithdrawal = 10,
    Reversal = 11
}
