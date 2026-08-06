using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.DependencyInjection;

namespace EHub.ShopManagement.Reports;

/// <summary>
/// Shared export logic reused by every report method, so no report duplicates its own Excel/CSV/print
/// building code. Excel is generated server-side with ClosedXML (already a project dependency). CSV is
/// generated server-side as plain text. There is no PDF-rendering library anywhere in this solution
/// (verified: no QuestPDF/iText/PdfSharp/DinkToPdf/etc. reference exists), so the "Pdf" format instead
/// returns the same clean, letterhead-styled HTML used for the browser print view - the user's browser
/// print dialog ("Save as PDF") produces the actual PDF file. This is a deliberate, documented choice
/// to avoid adding an unvetted/commercially-licensed dependency; swap this branch for a real PDF engine
/// later if binary PDF bytes become a hard requirement.
/// </summary>
public class ShopReportExportService : IShopReportExportService, ITransientDependency
{
    public Task<IRemoteStreamContent> ExportAsync(ShopReportExportRequest request, ShopReportExportFormat format, CancellationToken cancellationToken = default)
    {
        IRemoteStreamContent content = format switch
        {
            ShopReportExportFormat.Excel => BuildExcel(request),
            ShopReportExportFormat.Csv => BuildCsv(request),
            ShopReportExportFormat.Pdf => BuildPrintableHtml(request),
            _ => throw new BusinessException("ShopManagement:ReportExportFormatNotSupported"),
        };

        return Task.FromResult(content);
    }

    private static IRemoteStreamContent BuildExcel(ShopReportExportRequest request)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(Truncate(request.ReportTitle, 31));

        var row = 1;
        if (!string.IsNullOrWhiteSpace(request.ShopName))
        {
            sheet.Cell(row, 1).Value = request.ShopName;
            sheet.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 14;
            row++;
        }
        if (!string.IsNullOrWhiteSpace(request.ShopAddress)) { sheet.Cell(row, 1).Value = request.ShopAddress; row++; }
        if (!string.IsNullOrWhiteSpace(request.ShopPhone)) { sheet.Cell(row, 1).Value = request.ShopPhone; row++; }

        sheet.Cell(row, 1).Value = request.ReportTitle;
        sheet.Cell(row, 1).Style.Font.SetBold().Font.FontSize = 12;
        row++;

        if (request.DateFrom.HasValue && request.DateTo.HasValue)
        {
            sheet.Cell(row, 1).Value = $"Date Range: {request.DateFrom.Value:yyyy-MM-dd} to {request.DateTo.Value:yyyy-MM-dd}";
            row++;
        }

        sheet.Cell(row, 1).Value = $"Generated: {request.GeneratedDate:yyyy-MM-dd HH:mm}";
        row++;

        foreach (var filter in request.AppliedFilters)
        {
            sheet.Cell(row, 1).Value = filter;
            row++;
        }

        row++;
        var headerRow = row;
        for (var i = 0; i < request.Columns.Count; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = request.Columns[i].Header;
            cell.Style.Font.SetBold();
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDF4FA");
            cell.Style.Border.SetBottomBorder(XLBorderStyleValues.Thin);
        }
        row++;

        foreach (var dataRow in request.Rows)
        {
            for (var i = 0; i < request.Columns.Count; i++)
            {
                var value = dataRow.GetValueOrDefault(request.Columns[i].Key);
                SetCellValue(sheet.Cell(row, i + 1), value);
            }
            row++;
        }

        if (request.TotalsLines.Count > 0)
        {
            row++;
            foreach (var (label, value) in request.TotalsLines)
            {
                sheet.Cell(row, 1).Value = label;
                sheet.Cell(row, 1).Style.Font.SetBold();
                sheet.Cell(row, 2).Value = value;
                row++;
            }
        }

        sheet.Columns().AdjustToContents();

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        var fileName = $"{SanitizeFileName(request.ReportTitle)}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return new RemoteStreamContent(stream, fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
    }

    private static IRemoteStreamContent BuildCsv(ShopReportExportRequest request)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(request.ShopName)) sb.AppendLine(CsvEscape(request.ShopName));
        sb.AppendLine(CsvEscape(request.ReportTitle));
        if (request.DateFrom.HasValue && request.DateTo.HasValue)
            sb.AppendLine(CsvEscape($"Date Range: {request.DateFrom.Value:yyyy-MM-dd} to {request.DateTo.Value:yyyy-MM-dd}"));
        sb.AppendLine(CsvEscape($"Generated: {request.GeneratedDate:yyyy-MM-dd HH:mm}"));
        sb.AppendLine();

        sb.AppendLine(string.Join(",", request.Columns.Select(c => CsvEscape(c.Header))));

        foreach (var dataRow in request.Rows)
        {
            var line = request.Columns.Select(c => CsvEscape(FormatValue(dataRow.GetValueOrDefault(c.Key))));
            sb.AppendLine(string.Join(",", line));
        }

        if (request.TotalsLines.Count > 0)
        {
            sb.AppendLine();
            foreach (var (label, value) in request.TotalsLines)
                sb.AppendLine($"{CsvEscape(label)},{CsvEscape(value)}");
        }

        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        var stream = new MemoryStream(bytes);
        var fileName = $"{SanitizeFileName(request.ReportTitle)}_{DateTime.Now:yyyyMMddHHmmss}.csv";
        return new RemoteStreamContent(stream, fileName, "text/csv");
    }

    private static IRemoteStreamContent BuildPrintableHtml(ShopReportExportRequest request)
    {
        var sb = new StringBuilder();
        sb.Append("<!doctype html><html><head><meta charset=\"utf-8\"><title>").Append(WebEncode(request.ReportTitle)).Append("</title>");
        sb.Append("<style>body{font-family:Arial,sans-serif;font-size:12px;color:#222;margin:24px}h1{font-size:18px;margin:0 0 4px}h2{font-size:14px;margin:0 0 10px;color:#555}table{width:100%;border-collapse:collapse;margin-top:12px}th,td{border:1px solid #ddd;padding:6px 8px;text-align:left}th{background:#edf4fa}tfoot td{font-weight:bold;background:#f8fafc}.meta{color:#666;margin-bottom:2px}</style>");
        sb.Append("</head><body>");

        if (!string.IsNullOrWhiteSpace(request.ShopName)) sb.Append("<h1>").Append(WebEncode(request.ShopName)).Append("</h1>");
        if (!string.IsNullOrWhiteSpace(request.ShopAddress)) sb.Append("<div class=\"meta\">").Append(WebEncode(request.ShopAddress)).Append("</div>");
        if (!string.IsNullOrWhiteSpace(request.ShopPhone)) sb.Append("<div class=\"meta\">").Append(WebEncode(request.ShopPhone)).Append("</div>");

        sb.Append("<h2>").Append(WebEncode(request.ReportTitle)).Append("</h2>");
        if (request.DateFrom.HasValue && request.DateTo.HasValue)
            sb.Append("<div class=\"meta\">Date Range: ").Append(request.DateFrom.Value.ToString("yyyy-MM-dd")).Append(" to ").Append(request.DateTo.Value.ToString("yyyy-MM-dd")).Append("</div>");
        sb.Append("<div class=\"meta\">Generated: ").Append(request.GeneratedDate.ToString("yyyy-MM-dd HH:mm")).Append("</div>");

        foreach (var filter in request.AppliedFilters)
            sb.Append("<div class=\"meta\">").Append(WebEncode(filter)).Append("</div>");

        sb.Append("<table><thead><tr>");
        foreach (var col in request.Columns) sb.Append("<th>").Append(WebEncode(col.Header)).Append("</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (var dataRow in request.Rows)
        {
            sb.Append("<tr>");
            foreach (var col in request.Columns)
                sb.Append("<td>").Append(WebEncode(FormatValue(dataRow.GetValueOrDefault(col.Key)))).Append("</td>");
            sb.Append("</tr>");
        }

        sb.Append("</tbody>");

        if (request.TotalsLines.Count > 0)
        {
            sb.Append("<tfoot>");
            foreach (var (label, value) in request.TotalsLines)
                sb.Append("<tr><td colspan=\"").Append(Math.Max(1, request.Columns.Count - 1)).Append("\">").Append(WebEncode(label)).Append("</td><td>").Append(WebEncode(value)).Append("</td></tr>");
            sb.Append("</tfoot>");
        }

        sb.Append("</table>");
        sb.Append("<p style=\"margin-top:24px;color:#888;font-size:10px;\">Generated by Shop Management System</p>");
        sb.Append("</body></html>");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var stream = new MemoryStream(bytes);
        var fileName = $"{SanitizeFileName(request.ReportTitle)}_{DateTime.Now:yyyyMMddHHmmss}.html";
        return new RemoteStreamContent(stream, fileName, "text/html");
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = string.Empty;
                break;
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = "yyyy-mm-dd";
                break;
            case decimal dec:
                cell.Value = dec;
                cell.Style.NumberFormat.Format = "#,##0.00";
                break;
            case double dbl:
                cell.Value = dbl;
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static string FormatValue(object? value) => value switch
    {
        null => string.Empty,
        DateTime dt => dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        decimal dec => dec.ToString("N2", CultureInfo.InvariantCulture),
        _ => value.ToString() ?? string.Empty,
    };

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static string WebEncode(string value) => System.Net.WebUtility.HtmlEncode(value);

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value.Where(c => !invalid.Contains(c)).ToArray()).Replace(' ', '_');
        return string.IsNullOrWhiteSpace(cleaned) ? "Report" : cleaned;
    }

    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value.Substring(0, maxLength);
}
