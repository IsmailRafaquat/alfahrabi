using EHub.FeeModule.FeeStructureItems;
using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.FeeModule.FeeHeads;

public class FeeHead : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    public string Name { get; private set; }

    public bool IsActive { get; private set; } = true;
    public FeeHeadChargeType? ChargeType { get; private set; }
    //public virtual ICollection<FeeStructureItem> FeeStructureItems { get; set; } = new List<FeeStructureItem>();


    private FeeHead()
    {
    }

    public FeeHead(
        Guid id,
        string name,
        bool isActive = true,
        FeeHeadChargeType? chargeType = null
    ) : base(id)
    {
        SetName(name);
        IsActive = isActive;
        ChargeType = chargeType;
    }

    public FeeHead ChangeName(string name)
    {
        SetName(name);
        return this;
    }

    public FeeHead Activate()
    {
        IsActive = true;
        return this;
    }

    public FeeHead Deactivate()
    {
        IsActive = false;
        return this;
    }

    private void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(Name), maxLength: FeeModuleConsts.NameMaxLength);
    }

    public FeeHead SetChargeType(FeeHeadChargeType? chargeType)
    {
        ChargeType = chargeType;
        return this;
    }
}

