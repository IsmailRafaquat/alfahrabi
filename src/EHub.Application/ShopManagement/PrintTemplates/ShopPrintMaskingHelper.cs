namespace EHub.ShopManagement.PrintTemplates;

// Shared field-masking used by every mapper that touches a bank account number - never print a
// full account number on a receipt, only enough to identify which account it was.
public static class ShopPrintMaskingHelper
{
    public static string? MaskAccountNumber(string? accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            return null;

        var digitsOnly = accountNumber.Trim();
        return digitsOnly.Length <= 4
            ? new string('*', digitsOnly.Length)
            : new string('*', digitsOnly.Length - 4) + digitsOnly[^4..];
    }
}
