using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Suppliers;

public class ShopSupplier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public string? ContactPerson { get; protected set; }
    public string? Phone { get; protected set; }
    public string? AlternatePhone { get; protected set; }
    public string? Email { get; protected set; }

    public string? AddressLine1 { get; protected set; }
    public string? AddressLine2 { get; protected set; }
    public string? City { get; protected set; }
    public string? StateOrProvince { get; protected set; }
    public string? PostalCode { get; protected set; }
    public string? Country { get; protected set; }
    public string? TaxNumber { get; protected set; }

    public decimal OpeningBalance { get; protected set; }
    public decimal CreditLimit { get; protected set; }
    public int PaymentTermsDays { get; protected set; }
    public string? Notes { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopSupplier() { }

    internal ShopSupplier(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        string? contactPerson,
        string? phone,
        string? alternatePhone,
        string? email,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country,
        string? taxNumber,
        decimal openingBalance,
        decimal creditLimit,
        int paymentTermsDays,
        string? notes,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        SetValues(code, name, contactPerson, phone, alternatePhone, email, addressLine1, addressLine2, city,
            stateOrProvince, postalCode, country, taxNumber, openingBalance, creditLimit, paymentTermsDays, notes, isActive);
    }

    internal void Update(
        string code,
        string name,
        string? contactPerson,
        string? phone,
        string? alternatePhone,
        string? email,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country,
        string? taxNumber,
        decimal openingBalance,
        decimal creditLimit,
        int paymentTermsDays,
        string? notes,
        bool isActive) =>
        SetValues(code, name, contactPerson, phone, alternatePhone, email, addressLine1, addressLine2, city,
            stateOrProvince, postalCode, country, taxNumber, openingBalance, creditLimit, paymentTermsDays, notes, isActive);

    private void SetValues(
        string code,
        string name,
        string? contactPerson,
        string? phone,
        string? alternatePhone,
        string? email,
        string? addressLine1,
        string? addressLine2,
        string? city,
        string? stateOrProvince,
        string? postalCode,
        string? country,
        string? taxNumber,
        decimal openingBalance,
        decimal creditLimit,
        int paymentTermsDays,
        string? notes,
        bool isActive)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopSupplierConsts.CodeMaxLength).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopSupplierConsts.NameMaxLength).Trim();
        ContactPerson = Check.Length(contactPerson?.Trim(), nameof(contactPerson), ShopSupplierConsts.ContactPersonMaxLength);
        Phone = Check.Length(phone?.Trim(), nameof(phone), ShopSupplierConsts.PhoneMaxLength);
        AlternatePhone = Check.Length(alternatePhone?.Trim(), nameof(alternatePhone), ShopSupplierConsts.PhoneMaxLength);
        Email = Check.Length(email?.Trim(), nameof(email), ShopSupplierConsts.EmailMaxLength);
        AddressLine1 = Check.Length(addressLine1?.Trim(), nameof(addressLine1), ShopSupplierConsts.AddressLineMaxLength);
        AddressLine2 = Check.Length(addressLine2?.Trim(), nameof(addressLine2), ShopSupplierConsts.AddressLineMaxLength);
        City = Check.Length(city?.Trim(), nameof(city), ShopSupplierConsts.CityMaxLength);
        StateOrProvince = Check.Length(stateOrProvince?.Trim(), nameof(stateOrProvince), ShopSupplierConsts.StateOrProvinceMaxLength);
        PostalCode = Check.Length(postalCode?.Trim(), nameof(postalCode), ShopSupplierConsts.PostalCodeMaxLength);
        Country = Check.Length(country?.Trim(), nameof(country), ShopSupplierConsts.CountryMaxLength);
        TaxNumber = Check.Length(taxNumber?.Trim(), nameof(taxNumber), ShopSupplierConsts.TaxNumberMaxLength);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopSupplierConsts.NotesMaxLength);

        if (openingBalance < 0) throw new BusinessException("ShopManagement:SupplierOpeningBalanceCannotBeNegative");
        if (creditLimit < 0) throw new BusinessException("ShopManagement:SupplierCreditLimitCannotBeNegative");
        if (paymentTermsDays < 0) throw new BusinessException("ShopManagement:SupplierPaymentTermsCannotBeNegative");

        OpeningBalance = openingBalance;
        CreditLimit = creditLimit;
        PaymentTermsDays = paymentTermsDays;
        IsActive = isActive;
    }
}
