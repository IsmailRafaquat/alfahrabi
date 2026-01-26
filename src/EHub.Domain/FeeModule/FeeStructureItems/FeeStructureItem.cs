using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructures;
using System;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.FeeStructureItems;

[Audited]
public class FeeStructureItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid FeeStructureId { get; private set; }
    public FeeStructure FeeStructure { get; private set; } = default!;
    public Guid FeeHeadId { get; private set; }
    public FeeHead FeeHead { get; private set; } = default!;

    public decimal MonthlyAmount { get; private set; }
    public bool IsMandatory { get; private set; }

    private FeeStructureItem()
    {
        // ORM
    }

    public FeeStructureItem(
        Guid id,
        Guid feeStructureId,
        Guid feeHeadId,
        decimal monthlyAmount,
        bool isMandatory
    ) : base(id)
    {
        SetFeeStructureId(feeStructureId);
        SetFeeHeadId(feeHeadId);
        SetMonthlyAmount(monthlyAmount);
        IsMandatory = isMandatory;
    }

    public FeeStructureItem ChangeFeeHead(Guid feeHeadId)
    {
        SetFeeHeadId(feeHeadId);
        return this;
    }

    public FeeStructureItem ChangeMonthlyAmount(decimal monthlyAmount)
    {
        SetMonthlyAmount(monthlyAmount);
        return this;
    }

    public FeeStructureItem ChangeMandatory(bool isMandatory)
    {
        IsMandatory = isMandatory;
        return this;
    }

    private void SetFeeStructureId(Guid feeStructureId)
    {
        if (feeStructureId == Guid.Empty)
            throw new UserFriendlyException("FeeStructureId is required.");

        FeeStructureId = feeStructureId;
    }

    private void SetFeeHeadId(Guid feeHeadId)
    {
        if (feeHeadId == Guid.Empty)
            throw new UserFriendlyException("FeeHeadId is required.");

        FeeHeadId = feeHeadId;
    }

    private void SetMonthlyAmount(decimal monthlyAmount)
    {
        if (monthlyAmount < 0)
            throw new UserFriendlyException("Monthly amount must be greater than or equal to 0.");

        MonthlyAmount = monthlyAmount;
    }
}
