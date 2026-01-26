using EHub.Staffs;
using EHub.Students;
using System;

namespace EHub.StaffAttendances;

public class GenerateStaffAttendanceTemplateDto
{
    public Department Department { get; set; }
    public Shift? Shift { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}
