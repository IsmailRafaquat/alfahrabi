using System.ComponentModel.DataAnnotations;

namespace EHub.ShopManagement.Customers;

public class CreateUpdateShopCustomerDto
{
    [Required, StringLength(ShopCustomerConsts.CodeMaxLength)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(ShopCustomerConsts.NameMaxLength)]
    public string Name { get; set; } = string.Empty;

    public ShopCustomerType CustomerType { get; set; }

    [StringLength(ShopCustomerConsts.ContactPersonMaxLength)]
    public string? ContactPerson { get; set; }

    [StringLength(ShopCustomerConsts.PhoneMaxLength)]
    public string? Phone { get; set; }

    [StringLength(ShopCustomerConsts.PhoneMaxLength)]
    public string? AlternatePhone { get; set; }

    [EmailAddress, StringLength(ShopCustomerConsts.EmailMaxLength)]
    public string? Email { get; set; }

    [StringLength(ShopCustomerConsts.AddressLineMaxLength)]
    public string? AddressLine1 { get; set; }

    [StringLength(ShopCustomerConsts.AddressLineMaxLength)]
    public string? AddressLine2 { get; set; }

    [StringLength(ShopCustomerConsts.CityMaxLength)]
    public string? City { get; set; }

    [StringLength(ShopCustomerConsts.StateOrProvinceMaxLength)]
    public string? StateOrProvince { get; set; }

    [StringLength(ShopCustomerConsts.PostalCodeMaxLength)]
    public string? PostalCode { get; set; }

    [StringLength(ShopCustomerConsts.CountryMaxLength)]
    public string? Country { get; set; }

    [StringLength(ShopCustomerConsts.TaxNumberMaxLength)]
    public string? TaxNumber { get; set; }

    // Negative values are rejected by ShopCustomerManager as localized business rules
    // (ShopManagement:CustomerOpeningBalanceCannotBeNegative, etc.) rather than field
    // validation attributes, so callers get a friendly business exception.
    public decimal OpeningBalance { get; set; }
    public decimal CreditLimit { get; set; }
    public int PaymentTermsDays { get; set; }

    [StringLength(ShopCustomerConsts.NotesMaxLength)]
    public string? Notes { get; set; }

    public bool IsWalkInCustomer { get; set; }
    public bool IsActive { get; set; } = true;
}
