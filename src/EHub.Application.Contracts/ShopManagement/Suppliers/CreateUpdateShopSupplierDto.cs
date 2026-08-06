using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Suppliers;

public class CreateUpdateShopSupplierDto
{
    [Required, StringLength(ShopSupplierConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(ShopSupplierConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ShopSupplierConsts.ContactPersonMaxLength)]
    public string? ContactPerson { get; set; }

    [StringLength(ShopSupplierConsts.PhoneMaxLength)]
    public string? Phone { get; set; }

    [StringLength(ShopSupplierConsts.PhoneMaxLength)]
    public string? AlternatePhone { get; set; }

    [EmailAddress, StringLength(ShopSupplierConsts.EmailMaxLength)]
    public string? Email { get; set; }

    [StringLength(ShopSupplierConsts.AddressLineMaxLength)]
    public string? AddressLine1 { get; set; }

    [StringLength(ShopSupplierConsts.AddressLineMaxLength)]
    public string? AddressLine2 { get; set; }

    [StringLength(ShopSupplierConsts.CityMaxLength)]
    public string? City { get; set; }

    [StringLength(ShopSupplierConsts.StateOrProvinceMaxLength)]
    public string? StateOrProvince { get; set; }

    [StringLength(ShopSupplierConsts.PostalCodeMaxLength)]
    public string? PostalCode { get; set; }

    [StringLength(ShopSupplierConsts.CountryMaxLength)]
    public string? Country { get; set; }

    [StringLength(ShopSupplierConsts.TaxNumberMaxLength)]
    public string? TaxNumber { get; set; }

    // Negative values are rejected by ShopSupplierManager as localized business rules
    // (ShopManagement:SupplierOpeningBalanceCannotBeNegative, etc.) rather than field
    // validation attributes, so callers get a friendly business exception.
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }

    [StringLength(ShopSupplierConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
