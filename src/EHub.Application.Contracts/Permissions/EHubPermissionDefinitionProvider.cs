using EHub.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EHub.Permissions;

public class EHubPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(EHubPermissions.GroupName);

        myGroup.AddPermission(EHubPermissions.ManagementMenus.School, L("Permission:SchoolManagement"));
        var shopManagement = myGroup.AddPermission(EHubPermissions.ManagementMenus.Shop, L("Permission:ShopManagement"));
        var shopSettings = shopManagement.AddChild(EHubPermissions.ShopSettings.Default, L("Permission:ShopSettings"));
        shopSettings.AddChild(EHubPermissions.ShopSettings.Manage, L("Permission:ShopSettings.Manage"));
        var shopDashboard = shopManagement.AddChild(EHubPermissions.ShopDashboard.Default, L("Permission:ShopDashboard"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewFinancialSummary, L("Permission:ShopDashboard.ViewFinancialSummary"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewInventoryValue, L("Permission:ShopDashboard.ViewInventoryValue"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewBalances, L("Permission:ShopDashboard.ViewBalances"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewSalesChart, L("Permission:ShopDashboard.ViewSalesChart"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewTopProducts, L("Permission:ShopDashboard.ViewTopProducts"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewStockAlerts, L("Permission:ShopDashboard.ViewStockAlerts"));
        shopDashboard.AddChild(EHubPermissions.ShopDashboard.ViewRecentTransactions, L("Permission:ShopDashboard.ViewRecentTransactions"));
        var shopReports = shopManagement.AddChild(EHubPermissions.ShopReports.Default, L("Permission:ShopReports"));
        shopReports.AddChild(EHubPermissions.ShopReports.Sales, L("Permission:ShopReports.Sales"));
        shopReports.AddChild(EHubPermissions.ShopReports.Purchases, L("Permission:ShopReports.Purchases"));
        shopReports.AddChild(EHubPermissions.ShopReports.Stock, L("Permission:ShopReports.Stock"));
        shopReports.AddChild(EHubPermissions.ShopReports.StockMovements, L("Permission:ShopReports.StockMovements"));
        shopReports.AddChild(EHubPermissions.ShopReports.BatchExpiry, L("Permission:ShopReports.BatchExpiry"));
        shopReports.AddChild(EHubPermissions.ShopReports.CustomerReceivables, L("Permission:ShopReports.CustomerReceivables"));
        shopReports.AddChild(EHubPermissions.ShopReports.CustomerTransactions, L("Permission:ShopReports.CustomerTransactions"));
        shopReports.AddChild(EHubPermissions.ShopReports.SupplierPayables, L("Permission:ShopReports.SupplierPayables"));
        shopReports.AddChild(EHubPermissions.ShopReports.SupplierTransactions, L("Permission:ShopReports.SupplierTransactions"));
        shopReports.AddChild(EHubPermissions.ShopReports.Expenses, L("Permission:ShopReports.Expenses"));
        shopReports.AddChild(EHubPermissions.ShopReports.Cash, L("Permission:ShopReports.Cash"));
        shopReports.AddChild(EHubPermissions.ShopReports.Bank, L("Permission:ShopReports.Bank"));
        shopReports.AddChild(EHubPermissions.ShopReports.TaxSummary, L("Permission:ShopReports.TaxSummary"));
        shopReports.AddChild(EHubPermissions.ShopReports.ProductPerformance, L("Permission:ShopReports.ProductPerformance"));
        shopReports.AddChild(EHubPermissions.ShopReports.ViewCost, L("Permission:ShopReports.ViewCost"));
        shopReports.AddChild(EHubPermissions.ShopReports.ViewProfitSensitiveData, L("Permission:ShopReports.ViewProfitSensitiveData"));
        shopReports.AddChild(EHubPermissions.ShopReports.Export, L("Permission:ShopReports.Export"));
        shopReports.AddChild(EHubPermissions.ShopReports.Print, L("Permission:ShopReports.Print"));
        var productCategories = shopManagement.AddChild(EHubPermissions.ShopProductCategories.Default, L("Permission:ShopProductCategories"));
        productCategories.AddChild(EHubPermissions.ShopProductCategories.Create, L("Permission:ShopProductCategories.Create"));
        productCategories.AddChild(EHubPermissions.ShopProductCategories.Edit, L("Permission:ShopProductCategories.Edit"));
        productCategories.AddChild(EHubPermissions.ShopProductCategories.Delete, L("Permission:ShopProductCategories.Delete"));
        var shopUnits = shopManagement.AddChild(EHubPermissions.ShopUnits.Default, L("Permission:ShopUnits"));
        shopUnits.AddChild(EHubPermissions.ShopUnits.Create, L("Permission:ShopUnits.Create"));
        shopUnits.AddChild(EHubPermissions.ShopUnits.Edit, L("Permission:ShopUnits.Edit"));
        shopUnits.AddChild(EHubPermissions.ShopUnits.Delete, L("Permission:ShopUnits.Delete"));
        var shopProducts = shopManagement.AddChild(EHubPermissions.ShopProducts.Default, L("Permission:ShopProducts"));
        shopProducts.AddChild(EHubPermissions.ShopProducts.Create, L("Permission:ShopProducts.Create"));
        shopProducts.AddChild(EHubPermissions.ShopProducts.Edit, L("Permission:ShopProducts.Edit"));
        shopProducts.AddChild(EHubPermissions.ShopProducts.Delete, L("Permission:ShopProducts.Delete"));
        shopProducts.AddChild(EHubPermissions.ShopProducts.ViewCost, L("Permission:ShopProducts.ViewCost"));
        var shopSuppliers = shopManagement.AddChild(EHubPermissions.ShopSuppliers.Default, L("Permission:ShopSuppliers"));
        shopSuppliers.AddChild(EHubPermissions.ShopSuppliers.Create, L("Permission:ShopSuppliers.Create"));
        shopSuppliers.AddChild(EHubPermissions.ShopSuppliers.Edit, L("Permission:ShopSuppliers.Edit"));
        shopSuppliers.AddChild(EHubPermissions.ShopSuppliers.Delete, L("Permission:ShopSuppliers.Delete"));
        shopSuppliers.AddChild(EHubPermissions.ShopSuppliers.ViewBalance, L("Permission:ShopSuppliers.ViewBalance"));
        var shopPurchaseOrders = shopManagement.AddChild(EHubPermissions.ShopPurchaseOrders.Default, L("Permission:ShopPurchaseOrders"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Create, L("Permission:ShopPurchaseOrders.Create"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Edit, L("Permission:ShopPurchaseOrders.Edit"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Delete, L("Permission:ShopPurchaseOrders.Delete"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Submit, L("Permission:ShopPurchaseOrders.Submit"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Approve, L("Permission:ShopPurchaseOrders.Approve"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Reject, L("Permission:ShopPurchaseOrders.Reject"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.Cancel, L("Permission:ShopPurchaseOrders.Cancel"));
        shopPurchaseOrders.AddChild(EHubPermissions.ShopPurchaseOrders.ViewCost, L("Permission:ShopPurchaseOrders.ViewCost"));
        var shopGoodsReceipts = shopManagement.AddChild(EHubPermissions.ShopGoodsReceipts.Default, L("Permission:ShopGoodsReceipts"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.Create, L("Permission:ShopGoodsReceipts.Create"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.Edit, L("Permission:ShopGoodsReceipts.Edit"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.Delete, L("Permission:ShopGoodsReceipts.Delete"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.Complete, L("Permission:ShopGoodsReceipts.Complete"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.Cancel, L("Permission:ShopGoodsReceipts.Cancel"));
        shopGoodsReceipts.AddChild(EHubPermissions.ShopGoodsReceipts.ViewCost, L("Permission:ShopGoodsReceipts.ViewCost"));
        var shopStockTransactions = shopManagement.AddChild(EHubPermissions.ShopStockTransactions.Default, L("Permission:ShopStockTransactions"));
        shopStockTransactions.AddChild(EHubPermissions.ShopStockTransactions.ViewCost, L("Permission:ShopStockTransactions.ViewCost"));
        var shopStockAdjustments = shopManagement.AddChild(EHubPermissions.ShopStockAdjustments.Default, L("Permission:ShopStockAdjustments"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.Create, L("Permission:ShopStockAdjustments.Create"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.Edit, L("Permission:ShopStockAdjustments.Edit"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.Delete, L("Permission:ShopStockAdjustments.Delete"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.Post, L("Permission:ShopStockAdjustments.Post"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.Cancel, L("Permission:ShopStockAdjustments.Cancel"));
        shopStockAdjustments.AddChild(EHubPermissions.ShopStockAdjustments.ViewCost, L("Permission:ShopStockAdjustments.ViewCost"));
        var shopProductBatches = shopManagement.AddChild(EHubPermissions.ShopProductBatches.Default, L("Permission:ShopProductBatches"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.View, L("Permission:ShopProductBatches.View"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.ViewCost, L("Permission:ShopProductBatches.ViewCost"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.EditMetadata, L("Permission:ShopProductBatches.EditMetadata"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.Block, L("Permission:ShopProductBatches.Block"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.Unblock, L("Permission:ShopProductBatches.Unblock"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.ViewExpired, L("Permission:ShopProductBatches.ViewExpired"));
        shopProductBatches.AddChild(EHubPermissions.ShopProductBatches.ViewTransactions, L("Permission:ShopProductBatches.ViewTransactions"));
        var shopStockCounts = shopManagement.AddChild(EHubPermissions.ShopStockCounts.Default, L("Permission:ShopStockCounts"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Create, L("Permission:ShopStockCounts.Create"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Edit, L("Permission:ShopStockCounts.Edit"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Delete, L("Permission:ShopStockCounts.Delete"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Start, L("Permission:ShopStockCounts.Start"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Count, L("Permission:ShopStockCounts.Count"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Complete, L("Permission:ShopStockCounts.Complete"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Post, L("Permission:ShopStockCounts.Post"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.Cancel, L("Permission:ShopStockCounts.Cancel"));
        shopStockCounts.AddChild(EHubPermissions.ShopStockCounts.ViewCost, L("Permission:ShopStockCounts.ViewCost"));
        var shopSupplierPayments = shopManagement.AddChild(EHubPermissions.ShopSupplierPayments.Default, L("Permission:ShopSupplierPayments"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.Create, L("Permission:ShopSupplierPayments.Create"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.Edit, L("Permission:ShopSupplierPayments.Edit"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.Delete, L("Permission:ShopSupplierPayments.Delete"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.Post, L("Permission:ShopSupplierPayments.Post"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.Cancel, L("Permission:ShopSupplierPayments.Cancel"));
        shopSupplierPayments.AddChild(EHubPermissions.ShopSupplierPayments.ViewAmount, L("Permission:ShopSupplierPayments.ViewAmount"));
        var shopPurchaseReturns = shopManagement.AddChild(EHubPermissions.ShopPurchaseReturns.Default, L("Permission:ShopPurchaseReturns"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.Create, L("Permission:ShopPurchaseReturns.Create"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.Edit, L("Permission:ShopPurchaseReturns.Edit"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.Delete, L("Permission:ShopPurchaseReturns.Delete"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.Complete, L("Permission:ShopPurchaseReturns.Complete"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.Cancel, L("Permission:ShopPurchaseReturns.Cancel"));
        shopPurchaseReturns.AddChild(EHubPermissions.ShopPurchaseReturns.ViewCost, L("Permission:ShopPurchaseReturns.ViewCost"));
        var shopSupplierLedger = shopManagement.AddChild(EHubPermissions.ShopSupplierLedger.Default, L("Permission:ShopSupplierLedger"));
        shopSupplierLedger.AddChild(EHubPermissions.ShopSupplierLedger.ViewAmounts, L("Permission:ShopSupplierLedger.ViewAmounts"));
        shopSupplierLedger.AddChild(EHubPermissions.ShopSupplierLedger.Export, L("Permission:ShopSupplierLedger.Export"));
        var shopCustomers = shopManagement.AddChild(EHubPermissions.ShopCustomers.Default, L("Permission:ShopCustomers"));
        shopCustomers.AddChild(EHubPermissions.ShopCustomers.Create, L("Permission:ShopCustomers.Create"));
        shopCustomers.AddChild(EHubPermissions.ShopCustomers.Edit, L("Permission:ShopCustomers.Edit"));
        shopCustomers.AddChild(EHubPermissions.ShopCustomers.Delete, L("Permission:ShopCustomers.Delete"));
        shopCustomers.AddChild(EHubPermissions.ShopCustomers.ViewBalance, L("Permission:ShopCustomers.ViewBalance"));
        var shopCustomerLedger = shopManagement.AddChild(EHubPermissions.ShopCustomerLedger.Default, L("Permission:ShopCustomerLedger"));
        shopCustomerLedger.AddChild(EHubPermissions.ShopCustomerLedger.ViewAmounts, L("Permission:ShopCustomerLedger.ViewAmounts"));
        shopCustomerLedger.AddChild(EHubPermissions.ShopCustomerLedger.Print, L("Permission:ShopCustomerLedger.Print"));
        var shopCustomerPayments = shopManagement.AddChild(EHubPermissions.ShopCustomerPayments.Default, L("Permission:ShopCustomerPayments"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.Create, L("Permission:ShopCustomerPayments.Create"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.Edit, L("Permission:ShopCustomerPayments.Edit"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.Delete, L("Permission:ShopCustomerPayments.Delete"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.Post, L("Permission:ShopCustomerPayments.Post"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.Cancel, L("Permission:ShopCustomerPayments.Cancel"));
        shopCustomerPayments.AddChild(EHubPermissions.ShopCustomerPayments.ViewAmount, L("Permission:ShopCustomerPayments.ViewAmount"));
        var shopSales = shopManagement.AddChild(EHubPermissions.ShopSales.Default, L("Permission:ShopSales"));
        shopSales.AddChild(EHubPermissions.ShopSales.Create, L("Permission:ShopSales.Create"));
        shopSales.AddChild(EHubPermissions.ShopSales.Edit, L("Permission:ShopSales.Edit"));
        shopSales.AddChild(EHubPermissions.ShopSales.Delete, L("Permission:ShopSales.Delete"));
        shopSales.AddChild(EHubPermissions.ShopSales.Complete, L("Permission:ShopSales.Complete"));
        shopSales.AddChild(EHubPermissions.ShopSales.Cancel, L("Permission:ShopSales.Cancel"));
        shopSales.AddChild(EHubPermissions.ShopSales.ViewPrice, L("Permission:ShopSales.ViewPrice"));
        shopSales.AddChild(EHubPermissions.ShopSales.ViewCost, L("Permission:ShopSales.ViewCost"));
        var shopSaleReturns = shopManagement.AddChild(EHubPermissions.ShopSaleReturns.Default, L("Permission:ShopSaleReturns"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.Create, L("Permission:ShopSaleReturns.Create"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.Edit, L("Permission:ShopSaleReturns.Edit"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.Delete, L("Permission:ShopSaleReturns.Delete"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.Complete, L("Permission:ShopSaleReturns.Complete"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.Cancel, L("Permission:ShopSaleReturns.Cancel"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.ViewPrice, L("Permission:ShopSaleReturns.ViewPrice"));
        shopSaleReturns.AddChild(EHubPermissions.ShopSaleReturns.ViewCost, L("Permission:ShopSaleReturns.ViewCost"));
        var shopExpenseCategories = shopManagement.AddChild(EHubPermissions.ShopExpenseCategories.Default, L("Permission:ShopExpenseCategories"));
        shopExpenseCategories.AddChild(EHubPermissions.ShopExpenseCategories.Create, L("Permission:ShopExpenseCategories.Create"));
        shopExpenseCategories.AddChild(EHubPermissions.ShopExpenseCategories.Edit, L("Permission:ShopExpenseCategories.Edit"));
        shopExpenseCategories.AddChild(EHubPermissions.ShopExpenseCategories.Delete, L("Permission:ShopExpenseCategories.Delete"));
        var shopExpenses = shopManagement.AddChild(EHubPermissions.ShopExpenses.Default, L("Permission:ShopExpenses"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.Create, L("Permission:ShopExpenses.Create"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.Edit, L("Permission:ShopExpenses.Edit"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.Delete, L("Permission:ShopExpenses.Delete"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.Post, L("Permission:ShopExpenses.Post"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.Cancel, L("Permission:ShopExpenses.Cancel"));
        shopExpenses.AddChild(EHubPermissions.ShopExpenses.ViewAmount, L("Permission:ShopExpenses.ViewAmount"));
        var shopCashRegisters = shopManagement.AddChild(EHubPermissions.ShopCashRegisters.Default, L("Permission:ShopCashRegisters"));
        shopCashRegisters.AddChild(EHubPermissions.ShopCashRegisters.Create, L("Permission:ShopCashRegisters.Create"));
        shopCashRegisters.AddChild(EHubPermissions.ShopCashRegisters.Edit, L("Permission:ShopCashRegisters.Edit"));
        shopCashRegisters.AddChild(EHubPermissions.ShopCashRegisters.Delete, L("Permission:ShopCashRegisters.Delete"));
        var shopCashClosings = shopManagement.AddChild(EHubPermissions.ShopCashClosings.Default, L("Permission:ShopCashClosings"));
        shopCashClosings.AddChild(EHubPermissions.ShopCashClosings.Open, L("Permission:ShopCashClosings.Open"));
        shopCashClosings.AddChild(EHubPermissions.ShopCashClosings.Close, L("Permission:ShopCashClosings.Close"));
        shopCashClosings.AddChild(EHubPermissions.ShopCashClosings.Cancel, L("Permission:ShopCashClosings.Cancel"));
        shopCashClosings.AddChild(EHubPermissions.ShopCashClosings.ViewAmounts, L("Permission:ShopCashClosings.ViewAmounts"));
        var shopCashTransactions = shopManagement.AddChild(EHubPermissions.ShopCashTransactions.Default, L("Permission:ShopCashTransactions"));
        shopCashTransactions.AddChild(EHubPermissions.ShopCashTransactions.ManualMovement, L("Permission:ShopCashTransactions.ManualMovement"));
        shopCashTransactions.AddChild(EHubPermissions.ShopCashTransactions.ViewAmounts, L("Permission:ShopCashTransactions.ViewAmounts"));
        var shopBankAccounts = shopManagement.AddChild(EHubPermissions.ShopBankAccounts.Default, L("Permission:ShopBankAccounts"));
        shopBankAccounts.AddChild(EHubPermissions.ShopBankAccounts.Create, L("Permission:ShopBankAccounts.Create"));
        shopBankAccounts.AddChild(EHubPermissions.ShopBankAccounts.Edit, L("Permission:ShopBankAccounts.Edit"));
        shopBankAccounts.AddChild(EHubPermissions.ShopBankAccounts.Delete, L("Permission:ShopBankAccounts.Delete"));
        shopBankAccounts.AddChild(EHubPermissions.ShopBankAccounts.ViewBalance, L("Permission:ShopBankAccounts.ViewBalance"));
        var shopBankTransactions = shopManagement.AddChild(EHubPermissions.ShopBankTransactions.Default, L("Permission:ShopBankTransactions"));
        shopBankTransactions.AddChild(EHubPermissions.ShopBankTransactions.ManualMovement, L("Permission:ShopBankTransactions.ManualMovement"));
        shopBankTransactions.AddChild(EHubPermissions.ShopBankTransactions.ViewAmount, L("Permission:ShopBankTransactions.ViewAmount"));
        var shopBankTransfers = shopManagement.AddChild(EHubPermissions.ShopBankTransfers.Default, L("Permission:ShopBankTransfers"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.Create, L("Permission:ShopBankTransfers.Create"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.Edit, L("Permission:ShopBankTransfers.Edit"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.Delete, L("Permission:ShopBankTransfers.Delete"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.Post, L("Permission:ShopBankTransfers.Post"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.Cancel, L("Permission:ShopBankTransfers.Cancel"));
        shopBankTransfers.AddChild(EHubPermissions.ShopBankTransfers.ViewAmount, L("Permission:ShopBankTransfers.ViewAmount"));


        var shopAiAssistant = shopManagement.AddChild(EHubPermissions.ShopAiAssistant.Default, L("Permission:ShopAiAssistant"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.Use, L("Permission:ShopAiAssistant.Use"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.UseVoice, L("Permission:ShopAiAssistant.UseVoice"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.ViewHistory, L("Permission:ShopAiAssistant.ViewHistory"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.QuerySales, L("Permission:ShopAiAssistant.QuerySales"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.QueryExpenses, L("Permission:ShopAiAssistant.QueryExpenses"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.QueryStock, L("Permission:ShopAiAssistant.QueryStock"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.QueryBalances, L("Permission:ShopAiAssistant.QueryBalances"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.QueryDashboard, L("Permission:ShopAiAssistant.QueryDashboard"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreateCustomer, L("Permission:ShopAiAssistant.CreateCustomer"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreateSupplier, L("Permission:ShopAiAssistant.CreateSupplier"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreateProductDraft, L("Permission:ShopAiAssistant.CreateProductDraft"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreateExpenseDraft, L("Permission:ShopAiAssistant.CreateExpenseDraft"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreatePurchaseOrderDraft, L("Permission:ShopAiAssistant.CreatePurchaseOrderDraft"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.CreateSaleDraft, L("Permission:ShopAiAssistant.CreateSaleDraft"));
        shopAiAssistant.AddChild(EHubPermissions.ShopAiAssistant.ManageSettings, L("Permission:ShopAiAssistant.ManageSettings"));

        var dashboardPermission =
          myGroup.AddPermission(EHubPermissions.Dashboards.Dashboard, L("Permission:Dashboard"));

        var studentMainPermission =
          myGroup.AddPermission(EHubPermissions.StudentMenuItems.StudentMain, L("Permission:StudentMain"));

        var reportsPermission =
          myGroup.AddPermission(EHubPermissions.Reports.Default, L("Permission:Report"));

        studentMainPermission.AddChild(EHubPermissions.StudentMenuItems.StudentList, L("Permission:StudentList"));
        studentMainPermission.AddChild(EHubPermissions.StudentMenuItems.StudentAttendance, L("Permission:StudentAttendance"));
        studentMainPermission.AddChild(EHubPermissions.StudentMenuItems.StudentAttendanceInsights, L("Permission:StudentAttendanceInsights"));

        var studentsPermission = myGroup.AddPermission(EHubPermissions.Students.Default, L("Permission:Students"));
        studentsPermission.AddChild(EHubPermissions.Students.Create, L("Permission:Students.Create"));
        studentsPermission.AddChild(EHubPermissions.Students.Edit, L("Permission:Students.Edit"));
        studentsPermission.AddChild(EHubPermissions.Students.Delete, L("Permission:Students.Delete"));

        var staffMainPermission =
            myGroup.AddPermission(EHubPermissions.StaffMenuItems.StaffMain, L("Permission:StaffMain"));
        staffMainPermission.AddChild(EHubPermissions.StaffMenuItems.StaffList, L("Permission:StaffList"));
        staffMainPermission.AddChild(EHubPermissions.StaffMenuItems.StaffAttendance, L("Permission:StaffAttendance"));
        staffMainPermission.AddChild(EHubPermissions.StaffMenuItems.StaffAttendanceInsights, L("Permission:StaffAttendanceInsights"));

        var feeMainPermission =
            myGroup.AddPermission(EHubPermissions.FeeMenuItems.StudentMain, L("Permission:FeeMenu"));

        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.FeeHead, L("Permission:FeeHead"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.FeeStructure, L("Permission:FeeStructure"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.FeeStructureItem, L("Permission:FeeStructureItem"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.StudentFeeProfiles, L("Permission:StudentFeeProfile"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.StudentFeeDiscount, L("Permission:StudentFeeDiscount"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.LateFeePolicie, L("Permission:LateFeePolicie"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.StudentMonthlyFee, L("Permission:StudentMonthlyFee"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.StudentMonthlyFeeLine, L("Permission:StudentMonthlyFeeLine"));
        feeMainPermission.AddChild(EHubPermissions.FeeMenuItems.CheckFee, L("Permission:CheckFee"));


        var expenseMainPermission = myGroup.AddPermission(EHubPermissions.Expenses.ExpenseMain, L("Permission:ExpensesMenu"));

        expenseMainPermission.AddChild(EHubPermissions.Expenses.ExpenseCategory, L("Permission:ExpenseCategory"));
        expenseMainPermission.AddChild(EHubPermissions.Expenses.ExpenseEntry, L("Permission:ExpenseEntry"));
        expenseMainPermission.AddChild(EHubPermissions.Expenses.StaffSalaryPayment, L("Permission:StaffSalaryPayment"));
        expenseMainPermission.AddChild(EHubPermissions.Expenses.ExpenseDashboard, L("Permission:ExpenseDashboard"));



        var staffsPermission = myGroup.AddPermission(EHubPermissions.Staffs.Default, L("Permission:Staffs"));
        staffsPermission.AddChild(EHubPermissions.Staffs.Create, L("Permission:Staffs.Create"));
        staffsPermission.AddChild(EHubPermissions.Staffs.Edit, L("Permission:Staffs.Edit"));
        staffsPermission.AddChild(EHubPermissions.Staffs.Delete, L("Permission:Staffs.Delete"));

        var feeHeadsPermission = myGroup.AddPermission(EHubPermissions.FeeHeads.Default, L("Permission:FeeHeads"));
        feeHeadsPermission.AddChild(EHubPermissions.FeeHeads.Create, L("Permission:FeeHeads.Create"));
        feeHeadsPermission.AddChild(EHubPermissions.FeeHeads.Edit, L("Permission:FeeHeads.Edit"));
        feeHeadsPermission.AddChild(EHubPermissions.FeeHeads.Delete, L("Permission:FeeHeads.Delete"));

        var feestructuresPermission = myGroup.AddPermission(EHubPermissions.FeeStructures.Default, L("Permission:FeeStructures"));
        feestructuresPermission.AddChild(EHubPermissions.FeeStructures.Create, L("Permission:FeeStructures.Create"));
        feestructuresPermission.AddChild(EHubPermissions.FeeStructures.Edit, L("Permission:FeeStructures.Edit"));
        feestructuresPermission.AddChild(EHubPermissions.FeeStructures.Delete, L("Permission:FeeStructures.Delete"));

        var feestructureItemsPermission = myGroup.AddPermission(EHubPermissions.FeeStructureItems.Default, L("Permission:FeestructureItems"));
        feestructureItemsPermission.AddChild(EHubPermissions.FeeStructureItems.Create, L("Permission:FeeStructureItems.Create"));
        feestructureItemsPermission.AddChild(EHubPermissions.FeeStructureItems.Edit, L("Permission:FeeStructureItems.Edit"));
        feestructureItemsPermission.AddChild(EHubPermissions.FeeStructureItems.Delete, L("Permission:FeeStructureItems.Delete"));

        var studentFeeProfilesPermission = myGroup.AddPermission(EHubPermissions.StudentFeeProfiles.Default, L("Permission:StudentFeeProfiles"));
        studentFeeProfilesPermission.AddChild(EHubPermissions.StudentFeeProfiles.Create, L("Permission:StudentFeeProfiles.Create"));
        studentFeeProfilesPermission.AddChild(EHubPermissions.StudentFeeProfiles.Edit, L("Permission:StudentFeeProfiles.Edit"));
        studentFeeProfilesPermission.AddChild(EHubPermissions.StudentFeeProfiles.Delete, L("Permission:StudentFeeProfiles.Delete"));

        var studentFeeDiscountsPermission = myGroup.AddPermission(EHubPermissions.StudentFeeDiscounts.Default, L("Permission:StudentFeeDiscounts"));
        studentFeeDiscountsPermission.AddChild(EHubPermissions.StudentFeeDiscounts.Create, L("Permission:StudentFeeDiscounts.Create"));
        studentFeeDiscountsPermission.AddChild(EHubPermissions.StudentFeeDiscounts.Edit, L("Permission:StudentFeeDiscounts.Edit"));
        studentFeeDiscountsPermission.AddChild(EHubPermissions.StudentFeeDiscounts.Delete, L("Permission:StudentFeeDiscounts.Delete"));

        var lateFeePoliciesPermission = myGroup.AddPermission(EHubPermissions.LateFeePolicies.Default, L("Permission:LateFeePolicies"));
        lateFeePoliciesPermission.AddChild(EHubPermissions.LateFeePolicies.Create, L("Permission:LateFeePolicies.Create"));
        lateFeePoliciesPermission.AddChild(EHubPermissions.LateFeePolicies.Edit, L("Permission:LateFeePolicies.Edit"));
        lateFeePoliciesPermission.AddChild(EHubPermissions.LateFeePolicies.Delete, L("Permission:LateFeePolicies.Delete"));

        var studentMonthlyFeesPermission = myGroup.AddPermission(EHubPermissions.StudentMonthlyFees.Default, L("Permission:StudentMonthlyFees"));
        studentMonthlyFeesPermission.AddChild(EHubPermissions.StudentMonthlyFees.Create, L("Permission:StudentMonthlyFees.Create"));
        studentMonthlyFeesPermission.AddChild(EHubPermissions.StudentMonthlyFees.Edit, L("Permission:StudentMonthlyFees.Edit"));
        studentMonthlyFeesPermission.AddChild(EHubPermissions.StudentMonthlyFees.Delete, L("Permission:StudentMonthlyFees.Delete"));

        var studentMonthlyFeeLinesPermission = myGroup.AddPermission(EHubPermissions.StudentMonthlyFeeLines.Default, L("Permission:StudentMonthlyFeeLines"));
        studentMonthlyFeeLinesPermission.AddChild(EHubPermissions.StudentMonthlyFeeLines.Create, L("Permission:StudentMonthlyFeeLines.Create"));
        studentMonthlyFeeLinesPermission.AddChild(EHubPermissions.StudentMonthlyFeeLines.Edit, L("Permission:StudentMonthlyFeeLines.Edit"));
        studentMonthlyFeeLinesPermission.AddChild(EHubPermissions.StudentMonthlyFeeLines.Delete, L("Permission:StudentMonthlyFeeLines.Delete"));

        var expenseCategoriesPermission = myGroup.AddPermission(EHubPermissions.ExpenseCategories.Default, L("Permission:ExpenseCategories"));
        expenseCategoriesPermission.AddChild(EHubPermissions.ExpenseCategories.Create, L("Permission:ExpenseCategories.Create"));
        expenseCategoriesPermission.AddChild(EHubPermissions.ExpenseCategories.Edit, L("Permission:ExpenseCategories.Edit"));
        expenseCategoriesPermission.AddChild(EHubPermissions.ExpenseCategories.Delete, L("Permission:ExpenseCategories.Delete"));
       
        var expenseEntriesPermission = myGroup.AddPermission(EHubPermissions.ExpenseEntries.Default, L("Permission:ExpenseEntries"));
        expenseEntriesPermission.AddChild(EHubPermissions.ExpenseEntries.Create, L("Permission:ExpenseEntries.Create"));
        expenseEntriesPermission.AddChild(EHubPermissions.ExpenseEntries.Edit, L("Permission:ExpenseEntries.Edit"));
        expenseEntriesPermission.AddChild(EHubPermissions.ExpenseEntries.Delete, L("Permission:ExpenseEntries.Delete"));

        var staffSalaryPaymentsPermission = myGroup.AddPermission(EHubPermissions.StaffSalaryPayments.Default, L("Permission:StaffSalaryPayments"));
        staffSalaryPaymentsPermission.AddChild(EHubPermissions.StaffSalaryPayments.Create, L("Permission:StaffSalaryPayments.Create"));
        staffSalaryPaymentsPermission.AddChild(EHubPermissions.StaffSalaryPayments.Edit, L("Permission:StaffSalaryPayments.Edit"));
        staffSalaryPaymentsPermission.AddChild(EHubPermissions.StaffSalaryPayments.Delete, L("Permission:StaffSalaryPayments.Delete"));

    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<EHubResource>(name);
    }
}
