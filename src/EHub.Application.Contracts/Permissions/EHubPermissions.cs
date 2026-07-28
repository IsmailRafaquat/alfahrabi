namespace EHub.Permissions;

public static class EHubPermissions
{
    public const string GroupName = "EHub";

    public static class ManagementMenus
    {
        public const string School = GroupName + ".SchoolManagement";
        public const string Shop = "ShopManagement";
    }

    public static class ShopSettings
    {
        public const string Default = ManagementMenus.Shop + ".Settings";
        public const string Manage = Default + ".Manage";
    }

    public static class ShopDashboard
    {
        public const string Default = ManagementMenus.Shop + ".Dashboard";
        public const string ViewFinancialSummary = Default + ".ViewFinancialSummary";
        public const string ViewInventoryValue = Default + ".ViewInventoryValue";
        public const string ViewBalances = Default + ".ViewBalances";
        public const string ViewSalesChart = Default + ".ViewSalesChart";
        public const string ViewTopProducts = Default + ".ViewTopProducts";
        public const string ViewStockAlerts = Default + ".ViewStockAlerts";
        public const string ViewRecentTransactions = Default + ".ViewRecentTransactions";
    }

    public static class ShopProductCategories
    {
        public const string Default = ManagementMenus.Shop + ".ProductCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    public static class ShopUnits
    {
        public const string Default = ManagementMenus.Shop + ".Units";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ShopProducts
    {
        public const string Default = ManagementMenus.Shop + ".Products";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopSuppliers
    {
        public const string Default = ManagementMenus.Shop + ".Suppliers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewBalance = Default + ".ViewBalance";
    }

    public static class ShopPurchaseOrders
    {
        public const string Default = ManagementMenus.Shop + ".PurchaseOrders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Submit = Default + ".Submit";
        public const string Approve = Default + ".Approve";
        public const string Reject = Default + ".Reject";
        public const string Cancel = Default + ".Cancel";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopGoodsReceipts
    {
        public const string Default = ManagementMenus.Shop + ".GoodsReceipts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Complete = Default + ".Complete";
        public const string Cancel = Default + ".Cancel";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopStockTransactions
    {
        public const string Default = ManagementMenus.Shop + ".StockTransactions";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopStockAdjustments
    {
        public const string Default = ManagementMenus.Shop + ".StockAdjustments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopStockCounts
    {
        public const string Default = ManagementMenus.Shop + ".StockCounts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Start = Default + ".Start";
        public const string Count = Default + ".Count";
        public const string Complete = Default + ".Complete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopProductBatches
    {
        public const string Default = ManagementMenus.Shop + ".ProductBatches";
        public const string View = Default + ".View";
        public const string ViewCost = Default + ".ViewCost";
        public const string EditMetadata = Default + ".EditMetadata";
        public const string Block = Default + ".Block";
        public const string Unblock = Default + ".Unblock";
        public const string ViewExpired = Default + ".ViewExpired";
        public const string ViewTransactions = Default + ".ViewTransactions";
    }

    public static class ShopSupplierPayments
    {
        public const string Default = ManagementMenus.Shop + ".SupplierPayments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAmount = Default + ".ViewAmount";
    }

    public static class ShopPurchaseReturns
    {
        public const string Default = ManagementMenus.Shop + ".PurchaseReturns";
        public const string Create = Default + ".Create"; public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete"; public const string Complete = Default + ".Complete";
        public const string Cancel = Default + ".Cancel"; public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopSupplierLedger
    {
        public const string Default = ManagementMenus.Shop + ".SupplierLedger";
        public const string ViewAmounts = Default + ".ViewAmounts";
        public const string Export = Default + ".Export";
    }

    public static class ShopSales
    {
        public const string Default = ManagementMenus.Shop + ".Sales";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Complete = Default + ".Complete";
        public const string Cancel = Default + ".Cancel";
        public const string ViewPrice = Default + ".ViewPrice";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopSaleReturns
    {
        public const string Default = ManagementMenus.Shop + ".SaleReturns";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Complete = Default + ".Complete";
        public const string Cancel = Default + ".Cancel";
        public const string ViewPrice = Default + ".ViewPrice";
        public const string ViewCost = Default + ".ViewCost";
    }

    public static class ShopCustomerLedger
    {
        public const string Default = ManagementMenus.Shop + ".CustomerLedger";
        public const string ViewAmounts = Default + ".ViewAmounts";
        public const string Print = Default + ".Print";
    }

    public static class ShopCustomerPayments
    {
        public const string Default = ManagementMenus.Shop + ".CustomerPayments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAmount = Default + ".ViewAmount";
    }

    public static class ShopCustomers
    {
        public const string Default = ManagementMenus.Shop + ".Customers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewBalance = Default + ".ViewBalance";
    }

    public static class ShopExpenseCategories
    {
        public const string Default = ManagementMenus.Shop + ".ExpenseCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ShopExpenses
    {
        public const string Default = ManagementMenus.Shop + ".Expenses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAmount = Default + ".ViewAmount";
    }

    public static class ShopCashRegisters
    {
        public const string Default = ManagementMenus.Shop + ".CashRegisters";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ShopCashClosings
    {
        public const string Default = ManagementMenus.Shop + ".CashClosings";
        public const string Open = Default + ".Open";
        public const string Close = Default + ".Close";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAmounts = Default + ".ViewAmounts";
    }

    public static class ShopCashTransactions
    {
        public const string Default = ManagementMenus.Shop + ".CashTransactions";
        public const string ManualMovement = Default + ".ManualMovement";
        public const string ViewAmounts = Default + ".ViewAmounts";
    }

    public static class ShopBankAccounts
    {
        public const string Default = ManagementMenus.Shop + ".BankAccounts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewBalance = Default + ".ViewBalance";
    }

    public static class ShopBankTransactions
    {
        public const string Default = ManagementMenus.Shop + ".BankTransactions";
        public const string ManualMovement = Default + ".ManualMovement";
        public const string ViewAmount = Default + ".ViewAmount";
    }

    public static class ShopBankTransfers
    {
        public const string Default = ManagementMenus.Shop + ".BankTransfers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Post = Default + ".Post";
        public const string Cancel = Default + ".Cancel";
        public const string ViewAmount = Default + ".ViewAmount";
    }

    public static class Dashboards
    {
        public const string Dashboard = GroupName + ".Dashboard";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Report";
    }

    public static class StudentMenuItems
    {
        public const string StudentMain = GroupName + ".StudentMenu";
        public const string StudentList = StudentMain + ".StudentList";
        public const string StudentAttendance = StudentMain + ".StudentAttendance";
        public const string StudentAttendanceInsights = StudentMain + ".StudentAttendanceInsights";
    }

    public static class FeeMenuItems
    {
        public const string StudentMain = GroupName + ".FeeMenu";
        public const string FeeHead = StudentMain + ".FeeHead";
        public const string FeeStructure = StudentMain + ".FeeStructure";
        public const string FeeStructureItem = StudentMain + ".FeeStructureItem";
        public const string StudentFeeProfiles = StudentMain + ".StudentFeeProfile";
        public const string StudentFeeDiscount = StudentMain + ".StudentFeeDiscount";
        public const string LateFeePolicie = StudentMain + ".LateFeePolicie";
        public const string StudentMonthlyFee = StudentMain + ".StudentMonthlyFee";
        public const string StudentMonthlyFeeLine = StudentMain + ".StudentMonthlyFeeLine";
        public const string CheckFee = StudentMain + ".CheckFee";
    }

    public static class Expenses
    {
        public const string ExpenseMain = GroupName + ".Expenses";
        public const string ExpenseCategory = ExpenseMain + ".ExpenseCategory";
        public const string ExpenseEntry = ExpenseMain + ".ExpenseEntry";
        public const string StaffSalaryPayment = ExpenseMain + ".StaffSalaryPayment";
        public const string ExpenseDashboard = ExpenseMain + ".ExpenseDashboard";
    }

    public static class Students
    {
        public const string Default = GroupName + ".Students";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StaffMenuItems
    {
        public const string StaffMain = GroupName + ".StaffMenu";
        public const string StaffList = StaffMain + ".StaffList";
        public const string StaffAttendance = StaffMain + ".StaffAttendance";
        public const string StaffAttendanceInsights = StaffMain + ".StaffAttendanceInsights";
    }

    public static class Staffs
    {
        public const string Default = GroupName + ".Staffs";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class FeeHeads
    {
        public const string Default = GroupName + ".FeeHeads";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class FeeStructures
    {
        public const string Default = GroupName + ".FeeStructures";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class FeeStructureItems
    {
        public const string Default = GroupName + ".FeeStructureItems";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StudentFeeProfiles
    {
        public const string Default = GroupName + ".StudentFeeProfiles";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StudentFeeDiscounts
    {
        public const string Default = GroupName + ".StudentFeeDiscounts";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class LateFeePolicies
    {
        public const string Default = GroupName + ".LateFeePolicies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StudentMonthlyFees
    {
        public const string Default = GroupName + ".StudentMonthlyFees";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StudentMonthlyFeeLines
    {
        public const string Default = GroupName + ".StudentMonthlyFeeLines";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class ExpenseCategories
    {
        public const string Default = GroupName + ".ExpenseCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    public static class ExpenseEntries
    {
        public const string Default = GroupName + ".ExpenseEntries";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class StaffSalaryPayments
    {
        public const string Default = GroupName + ".StaffSalaryPayments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }


}
