using System;
using System.ComponentModel.DataAnnotations;

namespace EHub.Expenses.StaffSalaryPayments;

public class CreateUpdateStaffSalaryPaymentDto
{
    [Required]
    public Guid StaffId { get; set; }

    [Required]
    public DateTime SalaryMonth { get; set; } // any day, backend will normalize to 1st

    [Range(0.01, double.MaxValue)]
    public decimal SalaryAmount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [StringLength(1024)]
    public string? Remarks { get; set; }
}
