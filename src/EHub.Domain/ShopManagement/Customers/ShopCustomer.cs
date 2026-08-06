using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Customers;

public class ShopCustomer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    public string Code { get; protected set; } = string.Empty;
    public string Name { get; protected set; } = string.Empty;
    public ShopCustomerType CustomerType { get; protected set; }
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
    public bool IsWalkInCustomer { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    protected ShopCustomer() { }

    internal ShopCustomer(
        Guid id,
        Guid tenantId,
        string code,
        string name,
        ShopCustomerType customerType,
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
        bool isWalkInCustomer,
        bool isActive) : base(id)
    {
        TenantId = tenantId;
        SetValues(code, name, customerType, contactPerson, phone, alternatePhone, email, addressLine1, addressLine2,
            city, stateOrProvince, postalCode, country, taxNumber, openingBalance, creditLimit, paymentTermsDays,
            notes, isWalkInCustomer, isActive);
    }

    internal void Update(
        string code,
        string name,
        ShopCustomerType customerType,
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
        bool isWalkInCustomer,
        bool isActive) =>
        SetValues(code, name, customerType, contactPerson, phone, alternatePhone, email, addressLine1, addressLine2,
            city, stateOrProvince, postalCode, country, taxNumber, openingBalance, creditLimit, paymentTermsDays,
            notes, isWalkInCustomer, isActive);

    private void SetValues(
        string code,
        string name,
        ShopCustomerType customerType,
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
        bool isWalkInCustomer,
        bool isActive)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), ShopCustomerConsts.CodeMaxLength).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ShopCustomerConsts.NameMaxLength).Trim();
        ContactPerson = Check.Length(contactPerson?.Trim(), nameof(contactPerson), ShopCustomerConsts.ContactPersonMaxLength);
        Phone = Check.Length(phone?.Trim(), nameof(phone), ShopCustomerConsts.PhoneMaxLength);
        AlternatePhone = Check.Length(alternatePhone?.Trim(), nameof(alternatePhone), ShopCustomerConsts.PhoneMaxLength);
        Email = Check.Length(email?.Trim(), nameof(email), ShopCustomerConsts.EmailMaxLength);
        AddressLine1 = Check.Length(addressLine1?.Trim(), nameof(addressLine1), ShopCustomerConsts.AddressLineMaxLength);
        AddressLine2 = Check.Length(addressLine2?.Trim(), nameof(addressLine2), ShopCustomerConsts.AddressLineMaxLength);
        City = Check.Length(city?.Trim(), nameof(city), ShopCustomerConsts.CityMaxLength);
        StateOrProvince = Check.Length(stateOrProvince?.Trim(), nameof(stateOrProvince), ShopCustomerConsts.StateOrProvinceMaxLength);
        PostalCode = Check.Length(postalCode?.Trim(), nameof(postalCode), ShopCustomerConsts.PostalCodeMaxLength);
        Country = Check.Length(country?.Trim(), nameof(country), ShopCustomerConsts.CountryMaxLength);
        TaxNumber = Check.Length(taxNumber?.Trim(), nameof(taxNumber), ShopCustomerConsts.TaxNumberMaxLength);
        Notes = Check.Length(notes?.Trim(), nameof(notes), ShopCustomerConsts.NotesMaxLength);

        if (openingBalance < 0) throw new BusinessException("ShopManagement:CustomerOpeningBalanceCannotBeNegative");
        if (creditLimit < 0) throw new BusinessException("ShopManagement:CustomerCreditLimitCannotBeNegative");
        if (paymentTermsDays < 0) throw new BusinessException("ShopManagement:CustomerPaymentTermsCannotBeNegative");

        CustomerType = isWalkInCustomer ? ShopCustomerType.WalkIn : customerType;
        OpeningBalance = openingBalance;
        CreditLimit = creditLimit;
        PaymentTermsDays = paymentTermsDays;
        IsWalkInCustomer = isWalkInCustomer;
        IsActive = isActive;
    }
}
