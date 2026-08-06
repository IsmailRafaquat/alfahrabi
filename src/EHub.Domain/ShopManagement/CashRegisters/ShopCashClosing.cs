using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashClosing : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public Guid CashRegisterId { get; protected set; }
    public ShopCashRegister? CashRegister { get; protected set; }

    public DateTime BusinessDate { get; protected set; }
    public ShopCashClosingStatus Status { get; protected set; } = ShopCashClosingStatus.Open;

    public decimal OpeningCash { get; protected set; }
    public decimal CashSales { get; protected set; }
    public decimal CustomerCashPayments { get; protected set; }
    public decimal SupplierCashPayments { get; protected set; }
    public decimal CashExpenses { get; protected set; }
    public decimal CustomerRefunds { get; protected set; }
    public decimal ManualCashIn { get; protected set; }
    public decimal ManualCashOut { get; protected set; }
    public decimal ExpectedClosingCash { get; protected set; }
    public decimal? ActualClosingCash { get; protected set; }
    public decimal? DifferenceAmount { get; protected set; }

    public string? Notes { get; protected set; }

    public Guid? OpenedByUserId { get; protected set; }
    public DateTime OpenedDate { get; protected set; }
    public Guid? ClosedByUserId { get; protected set; }
    public DateTime? ClosedDate { get; protected set; }
    public string? CancellationReason { get; protected set; }

    protected ShopCashClosing() { }

    internal ShopCashClosing(
        Guid id,
        Guid tenantId,
        Guid cashRegisterId,
        DateTime businessDate,
        decimal openingCash,
        string? notes,
        Guid? openedByUserId,
        DateTime openedDate) : base(id)
    {
        if (openingCash < 0) throw new BusinessException("ShopManagement:CashClosingOpeningCashCannotBeNegative");

        TenantId = tenantId;
        CashRegisterId = cashRegisterId;
        BusinessDate = businessDate;
        Status = ShopCashClosingStatus.Open;
        OpeningCash = openingCash;
        ExpectedClosingCash = openingCash;
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopCashRegisterConsts.NotesMaxLength);
        OpenedByUserId = openedByUserId;
        OpenedDate = openedDate;
    }

    internal void ApplyClosingTotals(
        decimal cashSales,
        decimal customerCashPayments,
        decimal supplierCashPayments,
        decimal cashExpenses,
        decimal customerRefunds,
        decimal manualCashIn,
        decimal manualCashOut,
        decimal expectedClosingCash,
        decimal actualClosingCash,
        string? notes,
        Guid? closedByUserId,
        DateTime closedDate)
    {
        EnsureOpen("ShopManagement:CashClosingCannotBeClosed");
        if (actualClosingCash < 0) throw new BusinessException("ShopManagement:CashClosingActualCashCannotBeNegative");

        CashSales = cashSales;
        CustomerCashPayments = customerCashPayments;
        SupplierCashPayments = supplierCashPayments;
        CashExpenses = cashExpenses;
        CustomerRefunds = customerRefunds;
        ManualCashIn = manualCashIn;
        ManualCashOut = manualCashOut;
        ExpectedClosingCash = expectedClosingCash;
        ActualClosingCash = actualClosingCash;
        DifferenceAmount = Math.Round(actualClosingCash - expectedClosingCash, 2, MidpointRounding.AwayFromZero);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopCashRegisterConsts.NotesMaxLength) ?? Notes;

        Status = ShopCashClosingStatus.Closed;
        ClosedByUserId = closedByUserId;
        ClosedDate = closedDate;
    }

    internal void MarkAsCancelled(string cancellationReason)
    {
        EnsureOpen("ShopManagement:CashClosingCannotBeCancelled");
        Status = ShopCashClosingStatus.Cancelled;
        CancellationReason = Check.NotNullOrWhiteSpace(cancellationReason, nameof(cancellationReason), ShopCashRegisterConsts.CancellationReasonMaxLength).Trim();
    }

    private void EnsureOpen(string errorCode)
    {
        if (Status != ShopCashClosingStatus.Open) throw new BusinessException(errorCode);
    }
}
