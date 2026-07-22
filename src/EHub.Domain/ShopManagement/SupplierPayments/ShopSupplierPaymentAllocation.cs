using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.GoodsReceipts;

namespace EHub.ShopManagement.SupplierPayments;

public class ShopSupplierPaymentAllocation : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid SupplierPaymentId { get; protected set; }
    public ShopSupplierPayment? SupplierPayment { get; protected set; }

    public Guid GoodsReceiptId { get; protected set; }
    public ShopGoodsReceipt? GoodsReceipt { get; protected set; }

    public decimal AllocatedAmount { get; protected set; }

    protected ShopSupplierPaymentAllocation() { }

    internal ShopSupplierPaymentAllocation(Guid id, Guid tenantId, Guid supplierPaymentId, Guid goodsReceiptId, decimal allocatedAmount) : base(id)
    {
        TenantId = tenantId;
        SupplierPaymentId = supplierPaymentId;
        GoodsReceiptId = goodsReceiptId;

        if (allocatedAmount <= 0) throw new BusinessException("ShopManagement:SupplierPaymentInvalidAllocationAmount");
        AllocatedAmount = allocatedAmount;
    }
}
