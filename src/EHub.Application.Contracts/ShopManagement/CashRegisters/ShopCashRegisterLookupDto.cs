using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegisterLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
