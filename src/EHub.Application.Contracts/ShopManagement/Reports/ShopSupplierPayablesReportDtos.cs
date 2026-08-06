using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Reports;

public class GetShopSupplierPayablesReportInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? SupplierId { get; set; }
    public bool? HasOutstandingBalance { get; set; }
    public bool? HasAdvanceBalance { get; set; }
    public decimal? MinimumBalance { get; set; }
    public decimal? MaximumBalance { get; set; }
    public bool IncludeInactiveSuppliers { get; set; }
    public DateTime? LastTransactionFrom { get; set; }
    public DateTime? LastTransactionTo { get; set; }
}

public class ShopSupplierPayableReportItemDto
{
    public Guid SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public decimal TotalPurchases { get; set; }
    public decimal PurchaseReturns { get; set; }
    public decimal NetPurchases { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal SupplierAdvance { get; set; }
    public decimal OutstandingBalance { get; set; }

    public DateTime? LastPurchaseDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public bool IsActive { get; set; }
}

public class ShopSupplierPayableReportTotalsDto
{
    public int SupplierCount { get; set; }
    public int SuppliersWithPayables { get; set; }
    public int SuppliersWithAdvance { get; set; }
    public decimal TotalPurchases { get; set; }
    public decimal TotalPurchaseReturns { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalPayables { get; set; }
    public decimal TotalSupplierAdvance { get; set; }
}

public class ShopSupplierPayableReportResultDto
{
    public List<ShopSupplierPayableReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopSupplierPayableReportTotalsDto Totals { get; set; } = new();
}
