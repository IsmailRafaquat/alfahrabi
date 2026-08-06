namespace EHub.ShopManagement.PrintTemplates;

// Every field is nullable-by-omission on the Angular side (rendered only if non-null) so a
// simple flat document (e.g. an Expense Voucher) can populate just NetAmount while a Sale
// populates the full breakdown - the shared totals component never assumes every field exists.
public class ShopPrintTotalsDto
{
    public decimal? SubTotal { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? OtherChargesAmount { get; set; }
    public decimal? AdjustmentAmount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public decimal? PendingAmount { get; set; }
    public decimal? ChangeAmount { get; set; }
}
