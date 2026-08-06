using System; using System.Collections.Generic; using System.Linq; using Volo.Abp; using Volo.Abp.Domain.Entities.Auditing; using Volo.Abp.MultiTenancy;
using EHub.ShopManagement.GoodsReceipts; using EHub.ShopManagement.Suppliers;
namespace EHub.ShopManagement.PurchaseReturns;
public class ShopPurchaseReturn : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; } public string PurchaseReturnNumber { get; protected set; } = string.Empty;
    public Guid SupplierId { get; protected set; } public ShopSupplier? Supplier { get; protected set; }
    public Guid GoodsReceiptId { get; protected set; } public ShopGoodsReceipt? GoodsReceipt { get; protected set; }
    public DateTime ReturnDate { get; protected set; } public ShopPurchaseReturnStatus Status { get; protected set; }
    public ShopPurchaseReturnReason Reason { get; protected set; } public string? ReasonDetails { get; protected set; }
    public decimal SubTotal { get; protected set; } public decimal TaxAmount { get; protected set; } public decimal OtherCharges { get; protected set; } public decimal GrandTotal { get; protected set; }
    public string? Notes { get; protected set; } public Guid? CompletedByUserId { get; protected set; } public DateTime? CompletedDate { get; protected set; }
    public Guid? CancelledByUserId { get; protected set; } public DateTime? CancelledDate { get; protected set; } public string? CancellationReason { get; protected set; }
    public ICollection<ShopPurchaseReturnItem> Items { get; protected set; } = new List<ShopPurchaseReturnItem>();
    protected ShopPurchaseReturn() { }
    public ShopPurchaseReturn(Guid id, Guid tenantId, string number, Guid supplierId, Guid goodsReceiptId, DateTime returnDate, ShopPurchaseReturnReason reason, string? details, decimal otherCharges, string? notes, List<ShopPurchaseReturnItem> items) : base(id)
    { TenantId=tenantId; PurchaseReturnNumber=Check.NotNullOrWhiteSpace(number,nameof(number),ShopPurchaseReturnConsts.NumberMaxLength); SupplierId=supplierId; GoodsReceiptId=goodsReceiptId; Status=ShopPurchaseReturnStatus.Draft; SetValues(returnDate,reason,details,otherCharges,notes,items); }
    public void Update(DateTime date, ShopPurchaseReturnReason reason, string? details, decimal otherCharges, string? notes, List<ShopPurchaseReturnItem> items) { EnsureDraft("Edited"); SetValues(date,reason,details,otherCharges,notes,items); }
    public void Complete(Guid userId, DateTime date) { EnsureDraft("Completed"); Status=ShopPurchaseReturnStatus.Completed; CompletedByUserId=userId; CompletedDate=date; }
    public void Cancel(Guid userId, DateTime date, string reason) { EnsureDraft("Cancelled"); Status=ShopPurchaseReturnStatus.Cancelled; CancelledByUserId=userId; CancelledDate=date; CancellationReason=Check.NotNullOrWhiteSpace(reason,nameof(reason),ShopPurchaseReturnConsts.CancellationReasonMaxLength); }
    public void EnsureDraft(string action) { if(Status!=ShopPurchaseReturnStatus.Draft) throw new BusinessException($"ShopManagement:PurchaseReturnCannotBe{action}"); }
    private void SetValues(DateTime date, ShopPurchaseReturnReason reason, string? details, decimal otherCharges, string? notes, List<ShopPurchaseReturnItem> items)
    { if(otherCharges<0) throw new BusinessException("ShopManagement:PurchaseReturnOtherChargesCannotBeNegative"); if(items.Count==0) throw new BusinessException("ShopManagement:PurchaseReturnRequiresItems"); ReturnDate=date; Reason=reason; ReasonDetails=Check.Length(details?.Trim(),nameof(details),ShopPurchaseReturnConsts.ReasonDetailsMaxLength); OtherCharges=otherCharges; Notes=Check.Length(notes?.Trim(),nameof(notes),ShopPurchaseReturnConsts.NotesMaxLength); Items.Clear(); foreach(var item in items) Items.Add(item); SubTotal=Round(Items.Sum(x=>x.LineSubTotal)); TaxAmount=Round(Items.Sum(x=>x.TaxAmount)); GrandTotal=Round(SubTotal+TaxAmount+OtherCharges); }
    private static decimal Round(decimal value)=>Math.Round(value,2,MidpointRounding.AwayFromZero);
}
