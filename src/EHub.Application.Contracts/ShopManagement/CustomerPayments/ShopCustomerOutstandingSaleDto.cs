using System;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerOutstandingSaleDto
{
    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? InitialPaidAmount { get; set; }
    public decimal? AdditionalPaidAmount { get; set; }
    public decimal? TotalPaidAmount { get; set; }
    public decimal? PendingAmount { get; set; }
}
