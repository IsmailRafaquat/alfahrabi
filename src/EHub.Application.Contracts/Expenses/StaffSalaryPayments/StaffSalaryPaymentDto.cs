using System;
using Volo.Abp.Application.Dtos;

namespace EHub.Expenses.StaffSalaryPayments;

public class StaffSalaryPaymentDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = default!; // for grid

    public DateTime SalaryMonth { get; set; }
    public decimal SalaryAmount { get; set; }
    public DateTime PaymentDate { get; set; }

    public string? Remarks { get; set; }
}
