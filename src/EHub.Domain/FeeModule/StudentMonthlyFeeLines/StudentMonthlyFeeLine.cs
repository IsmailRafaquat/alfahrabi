using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.StudentMonthlyFees;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.StudentMonthlyFeeLines;

[Audited]
public class StudentMonthlyFeeLine : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid StudentMonthlyFeeId { get; private set; }
    public virtual StudentMonthlyFee StudentMonthlyFee { get; private set; } = default!;

    public Guid FeeHeadId { get; private set; }
    public virtual FeeHead FeeHead { get; private set; } = default!;

    public decimal ExpectedAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal AdjustmentAmount { get; private set; }
    public decimal LateFeeAmount { get; private set; }
    public decimal PaidAmount { get; private set; }

    public decimal NetAmount { get; private set; }
    public decimal OutstandingAmount { get; private set; }

    private StudentMonthlyFeeLine() { }

    public StudentMonthlyFeeLine(
        Guid id,
        Guid studentMonthlyFeeId,
        Guid feeHeadId,
        decimal expectedAmount,
        decimal discountAmount,
        decimal adjustmentAmount,
        decimal lateFeeAmount,
        decimal paidAmount
    ) : base(id)
    {
        SetStudentMonthlyFeeId(studentMonthlyFeeId);
        SetFeeHeadId(feeHeadId);

        SetExpectedAmount(expectedAmount);
        SetDiscountAmount(discountAmount);
        SetAdjustmentAmount(adjustmentAmount);
        SetLateFeeAmount(lateFeeAmount);
        SetPaidAmount(paidAmount);

        Recalculate();
    }

    public void ChangeDiscount(decimal discountAmount)
    {
        SetDiscountAmount(discountAmount);
        Recalculate();
    }

    public void ChangeAdjustment(decimal adjustmentAmount)
    {
        SetAdjustmentAmount(adjustmentAmount);
        Recalculate();
    }

    public void ChangeLateFee(decimal lateFeeAmount)
    {
        SetLateFeeAmount(lateFeeAmount);
        Recalculate();
    }

    public void ApplyPayment(decimal paidAmount)
    {
        SetPaidAmount(paidAmount);
        Recalculate();
    }

    private void Recalculate()
    {
        // Net = Expected - Discount + Adjustment + LateFee
        NetAmount = ExpectedAmount - DiscountAmount + AdjustmentAmount + LateFeeAmount;
        if (NetAmount < 0) NetAmount = 0;

        OutstandingAmount = NetAmount - PaidAmount;
        if (OutstandingAmount < 0) OutstandingAmount = 0;
    }

    private void SetStudentMonthlyFeeId(Guid id)
    {
        if (id == Guid.Empty) throw new UserFriendlyException("StudentMonthlyFeeId is required.");
        StudentMonthlyFeeId = id;
    }

    private void SetFeeHeadId(Guid id)
    {
        if (id == Guid.Empty) throw new UserFriendlyException("FeeHeadId is required.");
        FeeHeadId = id;
    }

    private void SetExpectedAmount(decimal v)
    {
        if (v < 0) throw new UserFriendlyException("ExpectedAmount must be >= 0.");
        ExpectedAmount = v;
    }

    private void SetDiscountAmount(decimal v)
    {
        if (v < 0) throw new UserFriendlyException("DiscountAmount must be >= 0.");
        DiscountAmount = v;
    }

    private void SetAdjustmentAmount(decimal v)
    {
        // adjustment can be negative (manual +/-)
        AdjustmentAmount = v;
    }

    private void SetLateFeeAmount(decimal v)
    {
        if (v < 0) throw new UserFriendlyException("LateFeeAmount must be >= 0.");
        LateFeeAmount = v;
    }

    private void SetPaidAmount(decimal v)
    {
        if (v < 0) throw new UserFriendlyException("PaidAmount must be >= 0.");
        PaidAmount = v;
    }
}
