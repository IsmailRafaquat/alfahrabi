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
