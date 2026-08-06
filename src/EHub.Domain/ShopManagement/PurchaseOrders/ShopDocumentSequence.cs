using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.PurchaseOrders;

public class ShopDocumentSequence : AggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public string DocumentType { get; protected set; } = string.Empty;
    public int LastNumber { get; protected set; }

    protected ShopDocumentSequence() { }

    internal ShopDocumentSequence(Guid id, Guid tenantId, string documentType) : base(id)
    {
        TenantId = tenantId;
        DocumentType = documentType;
        LastNumber = 0;
    }

    internal int IncrementAndGet()
    {
        LastNumber++;
        return LastNumber;
    }
}
