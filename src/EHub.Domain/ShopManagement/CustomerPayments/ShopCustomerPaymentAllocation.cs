using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.Sales;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPaymentAllocation : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid CustomerPaymentId { get; protected set; }
    public ShopCustomerPayment? CustomerPayment { get; protected set; }

    public Guid SaleId { get; protected set; }
    public ShopSale? Sale { get; protected set; }

    public decimal AllocatedAmount { get; protected set; }

    protected ShopCustomerPaymentAllocation() { }

    internal ShopCustomerPaymentAllocation(Guid id, Guid tenantId, Guid customerPaymentId, Guid saleId, decimal allocatedAmount) : base(id)
    {
        TenantId = tenantId;
        CustomerPaymentId = customerPaymentId;
        SaleId = saleId;

        if (allocatedAmount <= 0) throw new BusinessException("ShopManagement:CustomerPaymentInvalidAllocationAmount");
        AllocatedAmount = allocatedAmount;
    }
}
