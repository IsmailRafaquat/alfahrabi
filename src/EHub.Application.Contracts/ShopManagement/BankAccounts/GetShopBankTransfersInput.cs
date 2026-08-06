using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.BankAccounts;

public class GetShopBankTransfersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public ShopBankTransferType? TransferType { get; set; }
    public ShopBankTransferStatus? Status { get; set; }
    public Guid? FromBankAccountId { get; set; }
    public Guid? ToBankAccountId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
