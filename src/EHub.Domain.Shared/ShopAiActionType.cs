namespace EHub.ShopManagement.AiAssistant;

/// <summary>
/// Only the values registered in the handler registry (see ShopAiActionHandlerRegistry) are actually
/// reachable at runtime - everything else is a defined contract for a future phase and will be rejected by
/// ShopAiCommandParser's allowlist check if the model ever returns it prematurely.
/// </summary>
public enum ShopAiActionType
{
    Unknown = 0,
    GeneralHelp = 1,
    MissingInformation = 2,

    GetTodaySales = 100,
    GetSalesSummary = 101,
    GetTodayExpenses = 102,
    GetCustomerBalance = 103,
    GetSupplierBalance = 104,
    GetProductStock = 105,
    GetLowStockProducts = 106,
    GetOutOfStockProducts = 107,
    GetNearExpiryBatches = 108,
    GetExpiredBatches = 109,
    GetRecentSales = 110,
    GetRecentPurchases = 111,
    GetCashBalance = 112,
    GetBankBalances = 113,
    GetCustomerReceivables = 114,
    GetSupplierPayables = 115,
    GetDashboardSummary = 116,

    // Read-only "list/count existing records" actions - distinct from ExplainModule/ListModuleFields
    // (which describe a module's metadata) and from the Create* write actions below. See
    // ShopAiReadDataVerbDetector for the deterministic intent-priority override that routes list/
    // count/search phrases here even when the model itself misclassifies them.
    GetUnits = 117,
    GetProductCategories = 118,
    GetProducts = 119,
    GetCustomers = 120,
    GetSuppliers = 121,
    GetExpenseCategories = 122,
    GetBankAccounts = 123,
    GetSales = 124,
    GetPurchaseOrders = 125,
    GetExpenses = 126,
    GetStockAdjustments = 127,
    GetPhysicalStockCounts = 128,

    CreateCustomer = 200,
    CreateSupplier = 201,
    CreateProductDraft = 202,
    CreateExpenseDraft = 203,
    CreatePurchaseOrderDraft = 204,
    CreateSaleDraft = 205,

    // Guided-creation additions (module metadata + slot-filling). ProductDraft/ExpenseDraft/
    // PurchaseOrderDraft/SaleDraft above stay as-is; these are the newly approved modules.
    CreateProductCategory = 210,
    CreateUnit = 211,
    CreateProduct = 212,
    CreateExpenseCategory = 213,
    CreateBankAccount = 214,
    CreateStockAdjustmentDraft = 215,
    CreatePhysicalStockCountDraft = 216
}
