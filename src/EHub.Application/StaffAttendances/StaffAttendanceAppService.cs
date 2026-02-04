using ClosedXML.Excel;
using EHub.AttendanceStatuss;
using EHub.Staffs;
using EHub.Students;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace EHub.StaffAttendances;

[RemoteService(isEnabled: false)]
public class StaffAttendanceAppService : ApplicationService, IStaffAttendanceAppService
{
    private readonly IStaffAttendanceRepository _repository;
    private readonly StaffAttendanceManager _manager;
    private readonly IStaffRepository _staffRepository;

    public StaffAttendanceAppService(
        IStaffAttendanceRepository repository,
        StaffAttendanceManager manager,
        IStaffRepository staffRepository)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _manager = manager;
    }

    public async Task<StaffAttendanceDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<StaffAttendance, StaffAttendanceDto>(entity);
    }

    public async Task<PagedResultDto<StaffAttendanceDto>> GetListAsync(GetStaffAttendanceListDto input)
    {
        var sorting = input.Sorting.IsNullOrWhiteSpace()
            ? "AttendanceDate desc"
            : input.Sorting!;

        var totalCount = await _repository.GetCountAsync(
            input.Filter,
            input.StaffId,
            input.DateFrom,
            input.DateTo,
            input.Status,
            input.FirstName,
            input.LastName,
            input.EmployeeCode
        );

        var items = await _repository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            sorting,
            input.Filter,
            input.StaffId,
            input.DateFrom,
            input.DateTo,
            input.Status,
            input.FirstName,
            input.LastName,
            input.EmployeeCode
        );

        return new PagedResultDto<StaffAttendanceDto>(
            totalCount,
            ObjectMapper.Map<List<StaffAttendance>, List<StaffAttendanceDto>>(items)
        );
    }

    public async Task<StaffAttendanceDto> MarkAsync(MarkStaffAttendanceDto input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNull(input.StaffId, nameof(input.StaffId));

        var entity = await _manager.MarkAsync(
            input.StaffId,
            input.AttendanceDate,
            input.Status,
            input.Remarks
        );

        return ObjectMapper.Map<StaffAttendance, StaffAttendanceDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IRemoteStreamContent> DownLoadTemplateAsync(GenerateStaffAttendanceTemplateDto input)
    {
        if (input.DateFrom.Date > input.DateTo.Date)
            throw new UserFriendlyException("Date From must be <= Date To.");

        var from = NormalizeDate(input.DateFrom);
        var to = NormalizeDate(input.DateTo);

        // Build included dates (skip Sundays)
        var dates = new List<DateTime>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Sunday)
                continue;

            dates.Add(d);
        }

        if (dates.Count == 0)
            throw new UserFriendlyException("Selected date range contains only Sundays. Please select another range.");

        if (dates.Count > 62)
            throw new UserFriendlyException("Date range is too large. Please select up to 62 days.");

        // Load staff for department (adjust if you filter differently)
        var staffQ = await _staffRepository.GetQueryableAsync();
        var staffList = await AsyncExecuter.ToListAsync(
            staffQ.Where(s => s.Department == input.Department)
        );

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance");

        const int headerRow = 4;
        var startCol = 3;

        // Header fixed columns
        ws.Cell(headerRow, 1).Value = "Employee Code";
        ws.Cell(headerRow, 2).Value = "Staff Name";

        // Date columns
        var currentCol = startCol;
        foreach (var day in dates)
        {
            ws.Cell(headerRow, currentCol).Value = day.ToString("dd-MMM");
            ws.Cell(headerRow, currentCol).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Column(currentCol).Width = 10;
            currentCol++;
        }

        // Determine last column for merging title/meta/legend (meta uses col H)
        var lastCol = Math.Max(8, currentCol - 1);

        // Title
        ws.Cell(1, 1).Value = "Staff Attendance Template";
        ws.Range(1, 1, 1, lastCol).Merge().Style.Font.SetBold().Font.SetFontSize(14);

        // Meta row
        ws.Cell(2, 1).Value = "Department:";
        ws.Cell(2, 2).Value = input.Department.ToString();
        ws.Cell(2, 5).Value = "From:";
        ws.Cell(2, 6).Value = from.ToString("yyyy-MM-dd");
        ws.Cell(2, 7).Value = "To:";
        ws.Cell(2, 8).Value = to.ToString("yyyy-MM-dd");

        // Legend
        ws.Cell(3, 1).Value = "Legend:";
        ws.Cell(3, 2).Value = "P=Present, A=Absent, L=Late, E=Excused, S=Sick, LV=Leave, H=Holiday";
        ws.Range(3, 2, 3, lastCol).Merge();
        ws.Range(3, 1, 3, lastCol).Style.Font.SetItalic().Font.SetFontSize(10);

        // Style header row
        ws.Range(headerRow, 1, headerRow, currentCol - 1).Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        var allowedCodes = new[] { "P", "A", "L", "E", "S", "LV", "H" };

        // Staff rows (default P)
        var firstDataRow = headerRow + 1;
        var row = firstDataRow;

        foreach (var s in staffList)
        {
            ws.Cell(row, 1).Value = s.EmployeeCode;
            ws.Cell(row, 2).Value = $"{s.FirstName} {s.LastName}";

            var col = startCol;
            foreach (var _ in dates)
            {
                ws.Cell(row, col).Value = "P";
                ws.Cell(row, col).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                col++;
            }

            row++;
        }

        // Dropdown validation
        var lastDataRow = row - 1;
        var lastDateCol = currentCol - 1;

        if (lastDataRow >= firstDataRow && lastDateCol >= startCol)
        {
            var listFormula = $"\"{string.Join(",", allowedCodes)}\"";
            var dvRange = ws.Range(firstDataRow, startCol, lastDataRow, lastDateCol);

            var dv = dvRange.SetDataValidation();
            dv.List(listFormula, true);
            dv.IgnoreBlanks = true;
            dv.InCellDropdown = true;
            dv.ShowErrorMessage = true;
            dv.ErrorTitle = "Invalid Attendance Code";
            dv.ErrorMessage = "Use dropdown values only: P, A, L, E, S, LV.";
        }

        // Freeze
        ws.SheetView.FreezeRows(headerRow);
        ws.SheetView.FreezeColumns(2);

        // Borders + widths
        var used = ws.RangeUsed();
        if (used != null)
            used.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

        ws.Columns(1, 1).Width = 18;
        ws.Columns(2, 2).Width = 28;

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var fileName = $"StaffAttendance_{input.Department}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx";
        return new RemoteStreamContent(
            ms,
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    public async Task<ImportStaffAttendanceResultDto> ImportFromExcelAsync(IRemoteStreamContent file)
    {
        await using var stream = file.GetStream();
        return await ImportFromExcelAsync(stream);
    }

    public async Task<ImportStaffAttendanceResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        var result = new ImportStaffAttendanceResultDto();

        using var wb = new XLWorkbook(fileStream);
        var ws = wb.Worksheets.FirstOrDefault(w => w.Name == "Attendance") ?? wb.Worksheets.First();

        const int headerRow = 4;
        const int startCol = 3;

        // Meta From/To (Row 2 Col 6 and Col 8)
        var fromText = ws.Cell(2, 6).GetString()?.Trim();
        var toText = ws.Cell(2, 8).GetString()?.Trim();

        if (!DateTime.TryParseExact(fromText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from))
            throw new UserFriendlyException("Invalid 'From' date in Excel template.");

        if (!DateTime.TryParseExact(toText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
            throw new UserFriendlyException("Invalid 'To' date in Excel template.");

        from = NormalizeDate(from);
        to = NormalizeDate(to);

        if (from.Date > to.Date)
            throw new UserFriendlyException("Excel template dates are invalid (From > To).");

        // Build included dates (skip Sundays)
        var dates = new List<DateTime>();
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
        {
            if (d.DayOfWeek == DayOfWeek.Sunday)
                continue;

            dates.Add(d);
        }

        if (dates.Count == 0)
            throw new UserFriendlyException("Selected date range contains only Sundays.");

        if (dates.Count > 62)
            throw new UserFriendlyException("Date range is too large. Please select up to 62 days.");

        result.DatesInFile = dates.Count;

        var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        if (lastRowUsed <= headerRow)
            return result;

        // Collect employee codes
        var employeeCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var r = headerRow + 1; r <= lastRowUsed; r++)
        {
            var code = ws.Cell(r, 1).GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(code))
                employeeCodes.Add(code);
        }

        result.StaffInFile = employeeCodes.Count;
        if (employeeCodes.Count == 0)
            return result;

        // Load staff in one query
        var staffQ = await _staffRepository.GetQueryableAsync();
        var staffList = await AsyncExecuter.ToListAsync(
            staffQ.Where(s => employeeCodes.Contains(s.EmployeeCode))
        );

        var staffMap = staffList
            .Where(s => !string.IsNullOrWhiteSpace(s.EmployeeCode))
            .GroupBy(s => s.EmployeeCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Process rows
        for (var r = headerRow + 1; r <= lastRowUsed; r++)
        {
            var code = ws.Cell(r, 1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(code))
                continue;

            if (!staffMap.TryGetValue(code, out var staff))
            {
                result.Errors.Add($"Row {r}: Staff not found for Employee Code '{code}'.");
                continue;
            }

            for (var i = 0; i < dates.Count; i++)
            {
                var attendanceDate = dates[i];
                var col = startCol + i;
                var cellCode = ws.Cell(r, col).GetString()?.Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(cellCode))
                    cellCode = "P";

                AttendanceStatus status;
                try
                {
                    status = MapCodeToStatus(cellCode);
                }
                catch (UserFriendlyException ex)
                {
                    result.Errors.Add($"Row {r}, Col {col} ({attendanceDate:yyyy-MM-dd}): {ex.Message}");
                    continue;
                }

                await _manager.MarkAsync(staff.Id, attendanceDate, status, remarks: null);
                result.RecordsUpserted++;
            }

            result.StaffMatched++;
        }

        return result;
    }

    private static AttendanceStatus MapCodeToStatus(string code)
    {
        return code switch
        {
            "P" => AttendanceStatus.Present,
            "A" => AttendanceStatus.Absent,
            "L" => AttendanceStatus.Late,
            "E" => AttendanceStatus.Excused,
            "S" => AttendanceStatus.Sick,
            "LV" => AttendanceStatus.Leave,
            "H" => AttendanceStatus.Holiday,
            _ => throw new UserFriendlyException($"Invalid attendance code '{code}'. Allowed: P, A, L, E, S, LV, H.")
        };
    }

    public async Task<StaffAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync(GetStaffAttendanceLeaderboardDto input)
    {
        Check.NotNull(input, nameof(input));
        if (input.Department <= 0) throw new UserFriendlyException("Department is required.");

        var count = input.Count <= 0 ? 10 : input.Count;
        count = Math.Min(count, 200);

        var from = input.DateFrom.HasValue
            ? NormalizeDate(input.DateFrom.Value)
            : NormalizeDate(DateTime.Today.AddDays(-30));

        var to = input.DateTo.HasValue
            ? NormalizeDate(input.DateTo.Value)
            : NormalizeDate(DateTime.Today);

        if (from.Date > to.Date)
            throw new UserFriendlyException("Date From must be <= Date To.");

        var staffQ = (await _staffRepository.GetQueryableAsync())
            .Where(s => s.Department == input.Department);

        var attendanceQ = (await _repository.GetQueryableAsync())
            .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to);

        // Left join: include staff even if 0 attendance records
        var baseQ =
            from s in staffQ
            join a in attendanceQ on s.Id equals a.StaffId into gj
            from a in gj.DefaultIfEmpty()
            group a by new { s.Id, s.EmployeeCode, s.FirstName, s.LastName } into g
            select new StaffAttendanceLeaderboardItemDto
            {
                StaffId = g.Key.Id,
                EmployeeCode = g.Key.EmployeeCode,
                FullName = ((g.Key.FirstName ?? "") + " " + (g.Key.LastName ?? "")).Trim(),

                TotalDays = g.Count(x => x != null),

                PresentDays = g.Count(x => x != null && x.Status == AttendanceStatus.Present),
                AbsentDays = g.Count(x => x != null && x.Status == AttendanceStatus.Absent),
                LateDays = g.Count(x => x != null && x.Status == AttendanceStatus.Late),

                ExcusedDays = g.Count(x => x != null && x.Status == AttendanceStatus.Excused),
                SickDays = g.Count(x => x != null && x.Status == AttendanceStatus.Sick),
                LeaveDays = g.Count(x => x != null && x.Status == AttendanceStatus.Leave),
                HolidayDays = g.Count(x => x != null && x.Status == AttendanceStatus.Holiday),

                AttendanceRate =
                    g.Count(x => x != null) == 0
                        ? 0
                        : (double)g.Count(x => x != null && x.Status == AttendanceStatus.Present) * 100.0
                            / g.Count(x => x != null)
            };

        var rankedQ = input.Order == AttendanceLeaderboardOrder.Top
            ? baseQ.OrderByDescending(x => x.AttendanceRate)
                   .ThenByDescending(x => x.PresentDays)
                   .ThenBy(x => x.FullName)
            : baseQ.OrderBy(x => x.AttendanceRate)
                   .ThenByDescending(x => x.AbsentDays)
                   .ThenBy(x => x.FullName);

        var items = await AsyncExecuter.ToListAsync(rankedQ.Take(count));

        // Summary distribution (only real attendance rows) + restrict to department staff
        var summaryQ =
            from a in attendanceQ
            join s in staffQ on a.StaffId equals s.Id
            group a by 1 into g
            select new
            {
                Total = g.Count(),

                Present = g.Count(x => x.Status == AttendanceStatus.Present),
                Absent = g.Count(x => x.Status == AttendanceStatus.Absent),
                Late = g.Count(x => x.Status == AttendanceStatus.Late),
                Excused = g.Count(x => x.Status == AttendanceStatus.Excused),
                Sick = g.Count(x => x.Status == AttendanceStatus.Sick),
                Leave = g.Count(x => x.Status == AttendanceStatus.Leave),
                Holiday = g.Count(x => x.Status == AttendanceStatus.Holiday),

                Other = g.Count(x =>
                    x.Status != AttendanceStatus.Present &&
                    x.Status != AttendanceStatus.Absent &&
                    x.Status != AttendanceStatus.Late &&
                    x.Status != AttendanceStatus.Excused &&
                    x.Status != AttendanceStatus.Sick &&
                    x.Status != AttendanceStatus.Leave && 
                    x.Status != AttendanceStatus.Holiday)
            };

        var summary = await AsyncExecuter.FirstOrDefaultAsync(summaryQ);

        return new StaffAttendanceLeaderboardDto
        {
            DateFrom = from,
            DateTo = to,

            TotalRecords = summary?.Total ?? 0,
            PresentRecords = summary?.Present ?? 0,
            AbsentRecords = summary?.Absent ?? 0,
            LateRecords = summary?.Late ?? 0,
            ExcusedRecords = summary?.Excused ?? 0,
            SickRecords = summary?.Sick ?? 0,
            LeaveRecords = summary?.Leave ?? 0,
            HolidayRecords = summary?.Holiday ?? 0,
            OtherRecords = summary?.Other ?? 0,

            Items = items
        };
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);
}
