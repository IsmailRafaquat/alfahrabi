using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace EHub.ShopManagement.Reports;

public class GetShopCustomerReceivablesReportInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CustomerId { get; set; }
    public bool? HasOutstandingBalance { get; set; }
    public bool? HasAdvanceBalance { get; set; }
    public decimal? MinimumBalance { get; set; }
    public decimal? MaximumBalance { get; set; }
    public bool IncludeInactiveCustomers { get; set; }
    public DateTime? LastTransactionFrom { get; set; }
    public DateTime? LastTransactionTo { get; set; }
}

public class ShopCustomerReceivableReportItemDto
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public decimal TotalSales { get; set; }
    public decimal SaleReturns { get; set; }
    public decimal NetSales { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal CustomerAdvance { get; set; }
    public decimal OutstandingBalance { get; set; }

    public DateTime? LastSaleDate { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? LastTransactionDate { get; set; }
    public bool IsActive { get; set; }
}

public class ShopCustomerReceivableReportTotalsDto
{
    public int CustomerCount { get; set; }
    public int CustomersWithReceivables { get; set; }
    public int CustomersWithAdvance { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalSaleReturns { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalReceivables { get; set; }
    public decimal TotalCustomerAdvance { get; set; }
}

public class ShopCustomerReceivableReportResultDto
{
    public List<ShopCustomerReceivableReportItemDto> Items { get; set; } = new();
    public long TotalCount { get; set; }
    public ShopCustomerReceivableReportTotalsDto Totals { get; set; } = new();
}
