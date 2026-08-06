using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashClosingDto : EntityDto<Guid>
{
    public Guid CashRegisterId { get; set; }
    public string CashRegisterCode { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = string.Empty;

    public DateTime BusinessDate { get; set; }
    public ShopCashClosingStatus Status { get; set; }

    public decimal? OpeningCash { get; set; }
    public decimal? CashSales { get; set; }
    public decimal? CustomerCashPayments { get; set; }
    public decimal? SupplierCashPayments { get; set; }
    public decimal? CashExpenses { get; set; }
    public decimal? CustomerRefunds { get; set; }
    public decimal? ManualCashIn { get; set; }
    public decimal? ManualCashOut { get; set; }
    public decimal? ExpectedClosingCash { get; set; }
    public decimal? ActualClosingCash { get; set; }
    public decimal? DifferenceAmount { get; set; }

    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public Guid? OpenedByUserId { get; set; }
    public DateTime OpenedDate { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public DateTime? ClosedDate { get; set; }

    public DateTime CreationTime { get; set; }
}
