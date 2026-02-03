using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.Expenses.StaffSalaryPayments;

public class StaffSalaryPayment : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StaffId { get; private set; }

    // Store month as first day of month (e.g., 2026-02-01)
    public DateTime SalaryMonth { get; private set; }

    public decimal SalaryAmount { get; private set; }

    public DateTime PaymentDate { get; private set; }

    public string? Remarks { get; private set; }

    private StaffSalaryPayment()
    {
    }

    public StaffSalaryPayment(
        Guid id,
        Guid staffId,
        DateTime salaryMonth,
        decimal salaryAmount,
        DateTime paymentDate,
        string? remarks = null
    ) : base(id)
    {
        SetStaff(staffId);
        SetSalaryMonth(salaryMonth);
        SetSalaryAmount(salaryAmount);
        SetPaymentDate(paymentDate);
        Remarks = remarks;
    }

    public StaffSalaryPayment Update(
        Guid staffId,
        DateTime salaryMonth,
        decimal salaryAmount,
        DateTime paymentDate,
        string? remarks
    )
    {
        SetStaff(staffId);
        SetSalaryMonth(salaryMonth);
        SetSalaryAmount(salaryAmount);
        SetPaymentDate(paymentDate);
        Remarks = remarks;
        return this;
    }

    private void SetStaff(Guid staffId)
    {
        StaffId = Check.NotNull(staffId, nameof(StaffId));
    }

    private void SetSalaryMonth(DateTime month)
    {
        // Normalize to first day of month
        SalaryMonth = new DateTime(month.Year, month.Month, 1);
    }

    private void SetSalaryAmount(decimal amount)
    {
        if (amount <= 0)
            throw new BusinessException("SalaryAmountMustBeGreaterThanZero");

        SalaryAmount = amount;
    }

    private void SetPaymentDate(DateTime date)
    {
        PaymentDate = date;
    }
}
