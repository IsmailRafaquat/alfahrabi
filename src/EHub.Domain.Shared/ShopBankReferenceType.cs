namespace EHub.ShopManagement.BankAccounts;

public enum ShopBankReferenceType
{
    OpeningBalance = 0,
    CustomerPayment = 1,
    SupplierPayment = 2,
    Expense = 3,
    SaleReturn = 4,
    CashRegisterTransfer = 5,
    BankTransfer = 6,
    ManualBankMovement = 7
}
