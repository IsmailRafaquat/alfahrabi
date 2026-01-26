using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Staffs;

public class StaffLookupDto : EntityDto<Guid>
{
    public string EmployeeCode { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
