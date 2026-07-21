using System;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Suppliers;

public class ShopSupplierDto : EntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? Email { get; set; }

    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? StateOrProvince { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? TaxNumber { get; set; }

    public decimal? OpeningBalance { get; set; }
    public decimal? CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }

    public DateTime CreationTime { get; set; }
}
