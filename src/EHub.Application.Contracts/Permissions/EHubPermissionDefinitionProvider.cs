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
