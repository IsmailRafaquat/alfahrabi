using ClosedXML.Excel;
using EHub.AttendanceStatuss;
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

namespace EHub.StudentAttendances;

[RemoteService(isEnabled: false)]
public class StudentAttendanceAppService : ApplicationService, IStudentAttendanceAppService
{
    private readonly IStudentAttendanceRepository _repository;
    private readonly StudentAttendanceManager _manager;
    private readonly IStudentRepository _studentRepository;

    public StudentAttendanceAppService(
        IStudentAttendanceRepository repository,
        StudentAttendanceManager manager,
        IStudentRepository studentRepository)
    {
        _repository = repository;
        _studentRepository = studentRepository;
        _manager = manager;
    }

    public async Task<StudentAttendanceDto> GetAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        return ObjectMapper.Map<StudentAttendance, StudentAttendanceDto>(entity);
    }

    public async Task<PagedResultDto<StudentAttendanceDto>> GetListAsync(GetStudentAttendanceListDto input)
    {
        var sorting = input.Sorting.IsNullOrWhiteSpace()
            ? "AttendanceDate desc"
            : input.Sorting!;

        var totalCount = await _repository.GetCountAsync(
            input.Filter,
            input.StudentId,
            input.DateFrom,
            input.DateTo,
            input.Status,
            input.FirstName,
            input.LastName,
            input.AdmissionNo
        );

        var items = await _repository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            sorting,
            input.Filter,
            input.StudentId,
            input.DateFrom,
            input.DateTo,
            input.Status,
            input.FirstName,
            input.LastName,
            input.AdmissionNo
        );

        return new PagedResultDto<StudentAttendanceDto>(
            totalCount,
            ObjectMapper.Map<List<StudentAttendance>, List<StudentAttendanceDto>>(items)
        );
    }

    public async Task<StudentAttendanceDto> MarkAsync(MarkStudentAttendanceDto input)
    {
        Check.NotNull(input, nameof(input));
        Check.NotNull(input.StudentId, nameof(input.StudentId));

        var entity = await _manager.MarkAsync(
            input.StudentId,
            input.AttendanceDate,
            input.Status,
            input.Remarks
        );

        return ObjectMapper.Map<StudentAttendance, StudentAttendanceDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<IRemoteStreamContent> DownLoadTemplateAsync(GenerateStudentAttendanceTemplateDto input)
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

        var students = await _studentRepository.GetByClassSectionAsync(input.GradeLevel, input.Section);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance");

        const int headerRow = 4;
        var startCol = 3;

        // Header fixed columns
        ws.Cell(headerRow, 1).Value = "Admission No";
        ws.Cell(headerRow, 2).Value = "Student Name";

        // Date columns
        var currentCol = startCol;
        foreach (var day in dates)
        {
            ws.Cell(headerRow, currentCol).Value = day.ToString("dd-MMM");
            ws.Cell(headerRow, currentCol).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            ws.Column(currentCol).Width = 10;
            currentCol++;
        }

        // Determine last column for merging title/meta/legend (must be at least 8 because meta uses col H)
        var lastCol = Math.Max(8, currentCol - 1);

        // Title (merge across full table width)
        ws.Cell(1, 1).Value = "Student Attendance Template";
        ws.Range(1, 1, 1, lastCol).Merge().Style.Font.SetBold().Font.SetFontSize(14);

        // Meta row
        ws.Cell(2, 1).Value = "Class:";
        ws.Cell(2, 2).Value = input.GradeLevel.ToString();
        ws.Cell(2, 3).Value = "Section:";
        ws.Cell(2, 4).Value = input.Section.ToString();
        ws.Cell(2, 5).Value = "From:";
        ws.Cell(2, 6).Value = from.ToString("yyyy-MM-dd");
        ws.Cell(2, 7).Value = "To:";
        ws.Cell(2, 8).Value = to.ToString("yyyy-MM-dd");

        // Legend (merge across full table width)
        ws.Cell(3, 1).Value = "Legend:";
        ws.Cell(3, 2).Value = "P=Present, A=Absent, L=Late, E=Excused, S=Sick, LV=Leave, H=Holiday";
        ws.Range(3, 2, 3, lastCol).Merge();
        ws.Range(3, 1, 3, lastCol).Style.Font.SetItalic().Font.SetFontSize(10);

        // Style header row
        ws.Range(headerRow, 1, headerRow, currentCol - 1).Style
            .Font.SetBold()
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Short codes allowed
        var allowedCodes = new[] { "P", "A", "L", "E", "S", "LV", "H" };

        // Students rows (default P)
        var firstDataRow = headerRow + 1;
        var row = firstDataRow;

        foreach (var s in students)
        {
            ws.Cell(row, 1).Value = s.AdmissionNo;
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

        // Dropdown validation (only if we have students + date columns)
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

        // Freeze top rows + left columns
        ws.SheetView.FreezeRows(headerRow);
        ws.SheetView.FreezeColumns(2);

        // Borders + widths
        var used = ws.RangeUsed();
        if (used != null)
            used.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);

        ws.Columns(1, 1).Width = 16;
        ws.Columns(2, 2).Width = 28;

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var fileName = $"Attendance_{input.GradeLevel}_{input.Section}_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx";
        return new RemoteStreamContent(
            ms,
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }


    public async Task<ImportStudentAttendanceResultDto> ImportFromExcelAsync(IRemoteStreamContent file)
    {
        await using var stream = file.GetStream();
        return await ImportFromExcelAsync(stream);
    }

    public async Task<ImportStudentAttendanceResultDto> ImportFromExcelAsync(Stream fileStream)
    {
        var result = new ImportStudentAttendanceResultDto();

        using var wb = new XLWorkbook(fileStream);
        var ws = wb.Worksheets.FirstOrDefault(w => w.Name == "Attendance") ?? wb.Worksheets.First();

        const int headerRow = 4;
        const int startCol = 3;

        // Read meta From/To from the template (Row 2 Col 6 and Col 8)
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

        // Build included dates (skip Sundays) - must match your export logic
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

        // Find last used row
        var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        if (lastRowUsed <= headerRow)
            return result;

        // Collect admission numbers
        var admissionNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var r = headerRow + 1; r <= lastRowUsed; r++)
        {
            var admissionNo = ws.Cell(r, 1).GetString()?.Trim();
            if (!string.IsNullOrWhiteSpace(admissionNo))
                admissionNos.Add(admissionNo);
        }

        result.StudentsInFile = admissionNos.Count;
        if (admissionNos.Count == 0)
            return result;

        // Load students in one query (fast)
        var studentQuery = await _studentRepository.GetQueryableAsync();
        var students = await AsyncExecuter.ToListAsync(
                studentQuery.Where(s => admissionNos.Contains(s.AdmissionNo))
            );

        var studentMap = students
            .Where(s => !string.IsNullOrWhiteSpace(s.AdmissionNo))
            .GroupBy(s => s.AdmissionNo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        // Process rows
        for (var r = headerRow + 1; r <= lastRowUsed; r++)
        {
            var admissionNo = ws.Cell(r, 1).GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(admissionNo))
                continue;

            if (!studentMap.TryGetValue(admissionNo, out var student))
            {
                result.Errors.Add($"Row {r}: Student not found for Admission No '{admissionNo}'.");
                continue;
            }

            // For each date column
            for (var i = 0; i < dates.Count; i++)
            {
                var attendanceDate = dates[i];               // already Sunday-skipped
                var col = startCol + i;
                var code = ws.Cell(r, col).GetString()?.Trim().ToUpperInvariant();

                // If teacher clears cell, treat as Present
                if (string.IsNullOrWhiteSpace(code))
                    code = "P";

                AttendanceStatus status;
                try
                {
                    status = MapCodeToStatus(code);
                }
                catch (UserFriendlyException ex)
                {
                    result.Errors.Add($"Row {r}, Col {col} ({attendanceDate:yyyy-MM-dd}): {ex.Message}");
                    continue;
                }

                // Upsert via Manager
                await _manager.MarkAsync(student.Id, attendanceDate, status, remarks: null);
                result.RecordsUpserted++;
            }

            result.StudentsMatched++;
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

    public async Task<StudentAttendanceLeaderboardDto> GetAttendanceLeaderboardAsync(GetAttendanceLeaderboardDto input)
    {
        Check.NotNull(input, nameof(input));
        if (input.GradeLevel <= 0) throw new UserFriendlyException("Class (GradeLevel) is required.");
        if (input.Section <= 0) throw new UserFriendlyException("Section is required.");

        var count = input.Count <= 0 ? 10 : input.Count;
        count = Math.Min(count, 200);

        // Defaults if frontend doesn't send dates:
        var from = input.DateFrom.HasValue
            ? NormalizeDate(input.DateFrom.Value)
            : NormalizeDate(DateTime.Today.AddDays(-30));

        var to = input.DateTo.HasValue
            ? NormalizeDate(input.DateTo.Value)
            : NormalizeDate(DateTime.Today);

        if (from.Date > to.Date)
            throw new UserFriendlyException("Date From must be <= Date To.");

        var studentsQ = (await _studentRepository.GetQueryableAsync())
            .Where(s => s.GradeLevel == input.GradeLevel && s.Section == input.Section);

        var attendanceQ = (await _repository.GetQueryableAsync())
            .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to);

        // Left join: include students even if they have 0 attendance records in range
        var baseQ =
            from s in studentsQ
            join a in attendanceQ on s.Id equals a.StudentId into gj
            from a in gj.DefaultIfEmpty()
            group a by new { s.Id, s.AdmissionNo, s.FirstName, s.LastName } into g
            select new StudentAttendanceLeaderboardItemDto
            {
                StudentId = g.Key.Id,
                AdmissionNo = g.Key.AdmissionNo,
                FullName = ((g.Key.FirstName ?? "") + " " + (g.Key.LastName ?? "")).Trim(),

                TotalDays = g.Count(x => x != null),

                PresentDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Present),
                AbsentDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Absent),
                LateDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Late),

                ExcusedDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Excused),
                SickDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Sick),
                LeaveDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Leave),
                HolidayDays = g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Holiday),

                AttendanceRate =
                    g.Count(x => x != null) == 0
                        ? 0
                        : (double)g.Count(x => x != null && x.Status == AttendanceStatuss.AttendanceStatus.Present) * 100.0
                            / g.Count(x => x != null)
            };

        // Ranking
        var rankedQ = input.Order == AttendanceLeaderboardOrder.Top
            ? baseQ.OrderByDescending(x => x.AttendanceRate)
                   .ThenByDescending(x => x.PresentDays)
                   .ThenBy(x => x.FullName)
            : baseQ.OrderBy(x => x.AttendanceRate)
                   .ThenByDescending(x => x.AbsentDays)
                   .ThenBy(x => x.FullName);

        var items = await AsyncExecuter.ToListAsync(rankedQ.Take(count));

        // Summary distribution for doughnut chart (only real attendance rows)
        var summaryQ =
     from a in attendanceQ
     join s in studentsQ on a.StudentId equals s.Id
     group a by 1 into g
     select new
     {
         Total = g.Count(),

         Present = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Present),
         Absent = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Absent),
         Late = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Late),
         Excused = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Excused),
         Sick = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Sick),
         Leave = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Leave),
         Holiday = g.Count(x => x.Status == AttendanceStatuss.AttendanceStatus.Holiday),

         Other = g.Count(x =>
             x.Status != AttendanceStatuss.AttendanceStatus.Present &&
             x.Status != AttendanceStatuss.AttendanceStatus.Absent &&
             x.Status != AttendanceStatuss.AttendanceStatus.Late &&
             x.Status != AttendanceStatuss.AttendanceStatus.Excused &&
             x.Status != AttendanceStatuss.AttendanceStatus.Sick &&
             x.Status != AttendanceStatuss.AttendanceStatus.Leave &&
             x.Status != AttendanceStatuss.AttendanceStatus.Holiday)
     };

        var summary = await AsyncExecuter.FirstOrDefaultAsync(summaryQ);

        return new StudentAttendanceLeaderboardDto
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

