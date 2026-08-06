using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Suppliers;

public class ShopSupplierLookupDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public int PaymentTermsDays { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
