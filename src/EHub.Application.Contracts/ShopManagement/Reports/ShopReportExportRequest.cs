using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace EHub.ShopManagement.Reports;

public class ShopReportExportColumn
{
    public ShopReportExportColumn(string header, string key)
    {
        Header = header;
        Key = key;
    }

    public string Header { get; set; }
    public string Key { get; set; }
}

/// <summary>
/// A provider-agnostic description of a report export: shop letterhead info, applied date range,
/// column headings, already-formatted row values (keyed by column Key), and a totals section -
/// independent of whether the final file is Excel, CSV, or a printable HTML/PDF view.
/// </summary>
public class ShopReportExportRequest
{
    public string ReportTitle { get; set; } = string.Empty;
    public string? ShopName { get; set; }
    public string? ShopAddress { get; set; }
    public string? ShopPhone { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime GeneratedDate { get; set; }
    public List<string> AppliedFilters { get; set; } = new();

    public List<ShopReportExportColumn> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public List<(string Label, string Value)> TotalsLines { get; set; } = new();
}

public interface IShopReportExportService
{
    Task<IRemoteStreamContent> ExportAsync(ShopReportExportRequest request, ShopReportExportFormat format, CancellationToken cancellationToken = default);
}
