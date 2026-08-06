namespace EHub.ShopManagement.PrintTemplates;

// One row of a document's item table (sale/purchase line, stock-adjustment line, etc.). Optional
// medicine/batch-tracking fields are only ever populated when the source product/batch actually
// carries that data - the mapper never invents values for fields that don't exist on the entity.
public class ShopPrintLineDto
{
    public string ProductName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public string? Unit { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public string? BatchNumber { get; set; }
    public System.DateTime? ExpiryDate { get; set; }
    public System.DateTime? ManufacturingDate { get; set; }
    public string? GenericName { get; set; }
    public string? DosageStrength { get; set; }
}
