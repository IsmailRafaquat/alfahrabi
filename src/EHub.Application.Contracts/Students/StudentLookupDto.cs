using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Students;

public class StudentLookupDto : EntityDto<Guid>
{
    public string AdmissionNo { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
}
