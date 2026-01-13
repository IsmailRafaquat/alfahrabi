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

    public static class Students
    {
        public const string Default = GroupName + ".Students";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
}
