using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransferDto : EntityDto<Guid>
{
    public string TransferNumber { get; set; } = string.Empty;
    public DateTime TransferDate { get; set; }
    public ShopBankTransferType TransferType { get; set; }

    public Guid? FromBankAccountId { get; set; }
    public string? FromBankAccountName { get; set; }

    public Guid? ToBankAccountId { get; set; }
    public string? ToBankAccountName { get; set; }

    public Guid? CashRegisterId { get; set; }
    public string? CashRegisterName { get; set; }

    public decimal? Amount { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public ShopBankTransferStatus Status { get; set; }

    public Guid? PostedByUserId { get; set; }
    public DateTime? PostedDate { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreationTime { get; set; }
}
