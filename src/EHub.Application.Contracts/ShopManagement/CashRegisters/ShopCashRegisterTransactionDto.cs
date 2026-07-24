using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegisterTransactionDto : EntityDto<Guid>
{
    public Guid CashRegisterId { get; set; }
    public string CashRegisterCode { get; set; } = string.Empty;
    public string CashRegisterName { get; set; } = string.Empty;

    public Guid? CashClosingId { get; set; }

    public DateTime TransactionDate { get; set; }
    public ShopCashTransactionType TransactionType { get; set; }
    public ShopCashDirection Direction { get; set; }
    public decimal? Amount { get; set; }

    public ShopCashReferenceType ReferenceType { get; set; }
    public Guid ReferenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal? RunningBalance { get; set; }

    public DateTime CreationTime { get; set; }
}
