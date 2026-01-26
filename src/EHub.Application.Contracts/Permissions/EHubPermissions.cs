namespace EHub.Permissions;

public static class EHubPermissions
{
    public const string GroupName = "EHub";

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


}
