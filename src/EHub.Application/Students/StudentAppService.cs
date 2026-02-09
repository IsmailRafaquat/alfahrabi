using ClosedXML.Excel;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.StudentAttendances;
using Microsoft.AspNetCore.Http;
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

namespace EHub.Students;

[RemoteService(IsEnabled = false)]
public class StudentAppService : ApplicationService, IStudentAppService
{
    private readonly IStudentRepository _studentRepository;
    private readonly StudentManager _studentManager;

    public StudentAppService(
        IStudentRepository studentRepository,
        StudentManager studentManager)
    {
        _studentRepository = studentRepository;
        _studentManager = studentManager;
    }

    public async Task<StudentDto> GetAsync(Guid id)
    {
        var entity = await _studentRepository.GetByIdAsync(id);
        return ObjectMapper.Map<Student, StudentDto>(entity!);
    }

    public async Task<PagedResultDto<StudentDto>> GetListAsync(GetStudentListDto input)
    {
        if (input.Sorting.IsNullOrWhiteSpace())
        {
            input.Sorting = nameof(Student.FirstName);
        }

        var totalCount = await _studentRepository.GetCountAsync(
            input.Filter,
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.DOB,
            input.Gender,
            input.Status
        );

        var list = await _studentRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter,
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.GradeLevel,
            input.Section,
            input.Shift,
            input.Term,
            input.DOB,
            input.Gender,
            input.Status
        );

        var items = list.Select(s => ObjectMapper.Map<Student, StudentDto>(s)).ToList();

        return new PagedResultDto<StudentDto>(totalCount, items);
    }

    public async Task<StudentDto> CreateAsync(CreateStudentDto input)
    {
        var student = await _studentManager.CreateAsync(
            input.AdmissionNo,
            input.FirstName,
            input.LastName,
            input.Gender,
            input.DOB,
            input.EnrollmentDate,
            input.GradeLevel,
            input.Section,
            input.Term,
            input.Shift,
            input.City,
            input.Province,
            input.StreetAddress,
            input.ZipCode,
            input.PFirstName,
            input.PLastName,
            input.Grade,
            input.PRelatonShipToStudent,
            input.PPhone,
            input.Status,
            input.Email,
            input.StreetAddressLine2,
            input.PEmail,
            input.ECFirstName,
            input.ECLastName,
            input.ECRelationShipToStudent,
            input.ECPhone,
            input.ECEmail,
            input.PerviousSchool,
            input.StudentIdNo,
            input.MedicalConditions,
            input.Extracurrucular,
            input.Commnets,
            input.Accommodations
        );

        student = await _studentRepository.InsertAsync(student);
        return ObjectMapper.Map<Student, StudentDto>(student);
    }

    public async Task UpdateAsync(Guid id, UpdateStudentDto input)
    {
        var student = await _studentRepository.GetAsync(id);

        // Update core info
        await _studentManager.ChangeNameAsync(student, input.FirstName, input.LastName);
        await _studentManager.ChangeContactsAsync(student, input.Email);
        await _studentManager.ChangeAddressAsync(student, input.StreetAddress, input.StreetAddressLine2, input.City, input.Province, input.ZipCode);
        await _studentManager.ChangeParentInfoAsync(student, input.PFirstName, input.PLastName, input.PRelatonShipToStudent, input.PPhone, input.PEmail);
        await _studentManager.ChangeEmergencyContactAsync(student, input.ECFirstName, input.ECLastName, input.ECRelationShipToStudent, input.ECPhone, input.ECEmail);
        await _studentManager.ChangeStatusAsync(student, input.Status);

        student.AdmissionNo = input.AdmissionNo;
        student.Grade = input.Grade;
        student.GradeLevel = input.GradeLevel;
        student.Gender = input.Gender;
        student.DOB = input.DOB;
        student.EnrollmentDate = input.EnrollmentDate;
        student.Status = input.Status;
        student.Term = input.Term;
        student.Shift = input.Shift;

        //Educational Background
        student.PerviousSchool = input.PerviousSchool;
        student.StudentIdNo = input.StudentIdNo;
        student.MedicalConditions = input.MedicalConditions;
        student.Extracurrucular = input.Extracurrucular;
        student.Commnets = input.Commnets;
        student.Accommodations = input.Accommodations;

        // Save
        await _studentRepository.UpdateAsync(student, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        var blockers = await _studentRepository.GetDeleteBlockersAsync(id);

        if (blockers.Count > 0)
        {
            throw new UserFriendlyException(
                $"This student cannot be deleted because it is used in: {string.Join(", ", blockers)}. " +
                "Please remove those related records first, then try again."
            );
        }

        await _studentRepository.DeleteAsync(id);
    }

    public async Task<List<StudentLookupDto>> GetStudentLookupAsync()
    {
        var students = await _studentRepository.GetStudentLookupAsync();

        return students.Select(x => new StudentLookupDto
        {
            Id = x.Id,
            AdmissionNo = x.AdmissionNo,
            FirstName = x.FirstName,
            LastName = x.LastName
        }).ToList();
    }


    //Excel work

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync(GenerateStudentImportTemplateDto input)
    {
        Check.NotNull(input, nameof(input));

        if (input.ExtraEmptyRows < 0) input.ExtraEmptyRows = 0;
        if (input.ExtraEmptyRows > 500) input.ExtraEmptyRows = 500;

        var students = new List<Student>();
        if (input.IncludeExistingStudents)
        {
            students = await _studentRepository.GetByClassSectionAsync(input.GradeLevel, input.Section);
        }

        using var wb = new XLWorkbook();

        // Hidden lists sheet for dropdowns
        var wsLists = wb.Worksheets.Add("Lists");
        BuildListsSheet(wsLists);

        // Instructions
        var wsHelp = wb.Worksheets.Add("Instructions");
        BuildInstructionsSheet(wsHelp);

        // Main
        var ws = wb.Worksheets.Add("Students");
        BuildStudentsSheet(ws, wsLists, input, students);

        wsLists.Hide();

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;

        var fileName = $"Students_Import_{input.GradeLevel}_{input.Section}_{DateTime.UtcNow:yyyyMMdd_HHmm}.xlsx";
        return new RemoteStreamContent(
            ms,
            fileName,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }

    private static void BuildListsSheet(IXLWorksheet ws)
    {
        // 1-based columns
        ws.Cell(1, 1).Value = "Gender";
        ws.Cell(1, 2).Value = "Status";
        ws.Cell(1, 3).Value = "Shift";
        ws.Cell(1, 4).Value = "Term";
        ws.Cell(1, 5).Value = "GradeLevel";
        ws.Cell(1, 6).Value = "Section";
        ws.Cell(1, 7).Value = "City";
        ws.Cell(1, 8).Value = "Province";
        ws.Cell(1, 9).Value = "RelationShipToStudent";

        WriteEnumColumn(ws, 1, 2, Enum.GetNames(typeof(Gender)));
        WriteEnumColumn(ws, 2, 2, Enum.GetNames(typeof(Status)));
        WriteEnumColumn(ws, 3, 2, Enum.GetNames(typeof(Shift)));
        WriteEnumColumn(ws, 4, 2, Enum.GetNames(typeof(Term)));
        WriteEnumColumn(ws, 5, 2, Enum.GetNames(typeof(GradeLevel)));
        WriteEnumColumn(ws, 6, 2, Enum.GetNames(typeof(Section)));
        WriteEnumColumn(ws, 7, 2, Enum.GetNames(typeof(City)));
        WriteEnumColumn(ws, 8, 2, Enum.GetNames(typeof(Province)));
        WriteEnumColumn(ws, 9, 2, Enum.GetNames(typeof(RelationShipToStudent)));

        ws.Columns().AdjustToContents();
    }

    private static void WriteEnumColumn(IXLWorksheet ws, int col, int startRow, string[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            ws.Cell(startRow + i, col).Value = values[i];
        }
    }

    private static void BuildInstructionsSheet(IXLWorksheet ws)
    {
        ws.Cell(1, 1).Value = "Student Import Template - Instructions";
        ws.Cell(1, 1).Style.Font.SetBold().Font.SetFontSize(14);

        var r = 3;

        ws.Cell(r++, 1).Value = "Fill ONLY the 'Students' sheet. Do not rename sheets.";
        ws.Cell(r++, 1).Value = "Use dropdowns for enum columns (Gender, City, Province, etc.). Do not type random values.";
        ws.Cell(r++, 1).Value = "Date format: yyyy-MM-dd (example: 2011-01-19).";
        ws.Cell(r++, 1).Value = "Required (minimum): AdmissionNo, FirstName, LastName, DOB, Gender, StreetAddress, City, Province, ZipCode, GradeLevel, Section, EnrollmentDate, Status, Parent fields.";
        ws.Cell(r++, 1).Value = "If AdmissionNo is empty, that row is treated as empty/ignored by importer (when implemented).";
        ws.Cell(r++, 1).Value = "AdmissionNo must be unique.";

        r++;
        ws.Cell(r, 1).Value = "Notes:";
        ws.Cell(r, 1).Style.Font.SetBold();
        r++;

        ws.Cell(r++, 1).Value = "Parent relationship and Emergency relationship are both RelationShipToStudent enum values.";
        ws.Cell(r++, 1).Value = "Emergency contact is optional; parent/guardian is mandatory per your domain rules.";

        ws.Columns(1, 1).Width = 120;
        ws.Rows().AdjustToContents();
    }

    private static void BuildStudentsSheet(
        IXLWorksheet ws,
        IXLWorksheet wsLists,
        GenerateStudentImportTemplateDto input,
        List<Student> existingStudents)
    {
        const int headerRow = 4;
        const int firstDataRow = 5;

        var columns = GetTemplateColumns();

        // Title
        ws.Cell(1, 1).Value = "Students Import Template";
        ws.Range(1, 1, 1, columns.Count).Merge();
        ws.Row(1).Style.Font.SetBold().Font.SetFontSize(14);

        // Meta
        ws.Cell(2, 1).Value = "Class (GradeLevel):";
        ws.Cell(2, 2).Value = input.GradeLevel.ToString();
        ws.Cell(2, 4).Value = "Section:";
        ws.Cell(2, 5).Value = input.Section.ToString();
        ws.Row(2).Style.Font.SetBold();

        // Header
        for (var c = 0; c < columns.Count; c++)
        {
            ws.Cell(headerRow, c + 1).Value = columns[c].Header;
            ws.Cell(headerRow, c + 1).Style.Font.SetBold();
        }

        ws.Range(headerRow, 1, headerRow, columns.Count).Style
            .Fill.SetBackgroundColor(XLColor.LightGray)
            .Alignment.SetVertical(XLAlignmentVerticalValues.Center);

        // Data
        var row = firstDataRow;

        if (existingStudents.Count == 0)
        {
            // One demo row (so user understands the format)
            WriteDemoRow(ws, row, columns, input);
            ws.Range(row, 1, row, columns.Count).Style.Fill.SetBackgroundColor(XLColor.FromArgb(255, 249, 229));
            ws.Range(row, 1, row, columns.Count).Style.Font.SetItalic();
            row += 2; // leave one empty line
        }
        else
        {
            foreach (var s in existingStudents.OrderBy(x => x.AdmissionNo))
            {
                WriteExistingStudentRow(ws, row, columns, s, input);
                row++;
            }

            // After existing, leave one empty row then blanks for new
            row++;
        }

        // Extra empty rows
        var lastRow = row + input.ExtraEmptyRows - 1;
        if (input.ExtraEmptyRows == 0)
            lastRow = Math.Max(firstDataRow, row - 1);

        // Apply validations for full data range including empty rows
        var dvFrom = firstDataRow;
        var dvTo = Math.Max(firstDataRow, lastRow);

        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Gender", listCol: 1);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Status", listCol: 2);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Shift", listCol: 3);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Term", listCol: 4);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "GradeLevel", listCol: 5);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Section", listCol: 6);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "City", listCol: 7);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "Province", listCol: 8);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "PRelatonShipToStudent", listCol: 9);
        ApplyDropdown(ws, wsLists, dvFrom, dvTo, columns, "ECRelationShipToStudent", listCol: 9);

        // Date formats
        SetDateFormat(ws, dvFrom, dvTo, columns, "DOB");
        SetDateFormat(ws, dvFrom, dvTo, columns, "EnrollmentDate");

        // Freeze
        ws.SheetView.FreezeRows(headerRow);
        ws.SheetView.FreezeColumns(3);

        // Widths
        ws.Columns().AdjustToContents();
        ws.Column(1).Width = 14; // AdmissionNo
        ws.Column(2).Width = 18; // FirstName
        ws.Column(3).Width = 18; // LastName
        ws.Column(4).Width = 14; // DOB

        // Borders
        var used = ws.RangeUsed();
        if (used != null)
        {
            used.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin)
                .Border.SetInsideBorder(XLBorderStyleValues.Thin);
        }
    }

    private static void ApplyDropdown(
        IXLWorksheet ws,
        IXLWorksheet wsLists,
        int fromRow,
        int toRow,
        List<TemplateCol> columns,
        string key,
        int listCol)
    {
        var colIndex = columns.FindIndex(x => x.Key == key) + 1;
        if (colIndex <= 0) return;

        var lastListRow = wsLists.Column(listCol).LastCellUsed()?.Address.RowNumber ?? 1;
        if (lastListRow < 2) return;

        var listLetter = XLHelper.GetColumnLetterFromNumber(listCol);
        var listRange = $"Lists!${listLetter}$2:${listLetter}${lastListRow}";

        var range = ws.Range(fromRow, colIndex, toRow, colIndex);
        var dv = range.SetDataValidation();
        dv.List(listRange, true);
        dv.IgnoreBlanks = true;
        dv.InCellDropdown = true;
        dv.ShowErrorMessage = true;
        dv.ErrorTitle = "Invalid value";
        dv.ErrorMessage = "Use the dropdown values only.";
    }

    private static void SetDateFormat(IXLWorksheet ws, int fromRow, int toRow, List<TemplateCol> columns, string key)
    {
        var colIndex = columns.FindIndex(x => x.Key == key) + 1;
        if (colIndex <= 0) return;

        ws.Range(fromRow, colIndex, toRow, colIndex).Style.DateFormat.Format = "yyyy-MM-dd";
    }

    private static List<TemplateCol> GetTemplateColumns()
    {
        // Must match your entity types:
        // - City, Province, RelationShipToStudent are enums -> dropdown
        // - GradeLevel, Section, Term, Shift, Gender, Status are enums -> dropdown
        // - DOB / EnrollmentDate are DateTime -> date format

        return new List<TemplateCol>
        {
            new("AdmissionNo", "Admission No"),
            new("FirstName", "First Name"),
            new("LastName", "Last Name"),
            new("DOB", "DOB"),
            new("Gender", "Gender"),
            new("Email", "Email"),

            new("StreetAddress", "Street Address"),
            new("StreetAddressLine2", "Street Address Line 2"),
            new("City", "City"),
            new("Province", "Province"),
            new("ZipCode", "Zip Code"),

            new("PFirstName", "Parent First Name"),
            new("PLastName", "Parent Last Name"),
            new("PRelatonShipToStudent", "Parent Relationship"),
            new("PPhone", "Parent Phone"),
            new("PEmail", "Parent Email"),

            new("ECFirstName", "Emergency First Name"),
            new("ECLastName", "Emergency Last Name"),
            new("ECRelationShipToStudent", "Emergency Relationship"),
            new("ECPhone", "Emergency Phone"),
            new("ECEmail", "Emergency Email"),

            new("GradeLevel", "GradeLevel"),
            new("Section", "Section"),
            new("EnrollmentDate", "Enrollment Date"),
            new("Status", "Status"),
            new("Term", "Term"),
            new("Shift", "Shift"),

            new("PerviousSchool", "Previous School"),
            new("Grade", "Previous GradeLevel"),
            new("StudentIdNo", "Student ID No"),

            new("MedicalConditions", "Medical Conditions"),
            new("Extracurrucular", "Extracurricular"),
            new("Commnets", "Comments"),
            new("Accommodations", "Accommodations"),
        };
    }

    private static void WriteExistingStudentRow(
        IXLWorksheet ws,
        int row,
        List<TemplateCol> columns,
        Student s,
        GenerateStudentImportTemplateDto input)
    {
        // NOTE: For GradeLevel/Section, I’m locking them to input to prevent editing into another class by mistake.
        // If you want them editable, use s.GradeLevel / s.Section.

        var values = new Dictionary<string, object?>
        {
            ["AdmissionNo"] = s.AdmissionNo,
            ["FirstName"] = s.FirstName,
            ["LastName"] = s.LastName,
            ["DOB"] = s.DOB,
            ["Gender"] = s.Gender.ToString(),
            ["Email"] = s.Email,

            ["StreetAddress"] = s.StreetAddress,
            ["StreetAddressLine2"] = s.StreetAddressLine2,
            ["City"] = s.City.ToString(),
            ["Province"] = s.Province.ToString(),
            ["ZipCode"] = s.ZipCode,

            ["PFirstName"] = s.PFirstName,
            ["PLastName"] = s.PLastName,
            ["PRelatonShipToStudent"] = s.PRelatonShipToStudent.ToString(),
            ["PPhone"] = s.PPhone,
            ["PEmail"] = s.PEmail,

            ["ECFirstName"] = s.ECFirstName,
            ["ECLastName"] = s.ECLastName,
            ["ECRelationShipToStudent"] = s.ECRelationShipToStudent?.ToString(),
            ["ECPhone"] = s.ECPhone,
            ["ECEmail"] = s.ECEmail,

            ["GradeLevel"] = input.GradeLevel.ToString(),
            ["Section"] = input.Section.ToString(),
            ["EnrollmentDate"] = s.EnrollmentDate,
            ["Status"] = s.Status.ToString(),
            ["Term"] = s.Term.ToString(),
            ["Shift"] = s.Shift.ToString(),

            ["PerviousSchool"] = s.PerviousSchool,
            ["Grade"] = s.Grade?.ToString(),
            ["StudentIdNo"] = s.StudentIdNo,

            ["MedicalConditions"] = s.MedicalConditions,
            ["Extracurrucular"] = s.Extracurrucular,
            ["Commnets"] = s.Commnets,
            ["Accommodations"] = s.Accommodations,
        };

        WriteRow(ws, row, columns, values);
    }

    private static void WriteDemoRow(IXLWorksheet ws, int row, List<TemplateCol> columns, GenerateStudentImportTemplateDto input)
    {
        var values = new Dictionary<string, object?>
        {
            ["AdmissionNo"] = "1001",
            ["FirstName"] = "Ismail",
            ["LastName"] = "Rafaquat",
            ["DOB"] = new DateTime(2011, 01, 19),
            ["Gender"] = Gender.Male.ToString(),
            ["Email"] = "ismail@example.com",

            ["StreetAddress"] = "Street 1",
            ["StreetAddressLine2"] = "",
            ["City"] = Enum.GetNames(typeof(City)).FirstOrDefault() ?? "",
            ["Province"] = Enum.GetNames(typeof(Province)).FirstOrDefault() ?? "",
            ["ZipCode"] = "44000",

            ["PFirstName"] = "ParentFirst",
            ["PLastName"] = "ParentLast",
            ["PRelatonShipToStudent"] = RelationShipToStudent.Father.ToString(),
            ["PPhone"] = "+92xxxxxxxxxx",
            ["PEmail"] = "parent@example.com",

            ["ECFirstName"] = "EmergencyFirst",
            ["ECLastName"] = "EmergencyLast",
            ["ECRelationShipToStudent"] = RelationShipToStudent.Uncle.ToString(),
            ["ECPhone"] = "+92xxxxxxxxxx",
            ["ECEmail"] = "emergency@example.com",

            ["GradeLevel"] = input.GradeLevel.ToString(),
            ["Section"] = input.Section.ToString(),
            ["EnrollmentDate"] = DateTime.Today,
            ["Status"] = Status.Active.ToString(),
            ["Term"] = Term.Fall.ToString(),
            ["Shift"] = Shift.Morning.ToString(),
        };

        WriteRow(ws, row, columns, values);
    }

    private static void WriteRow(IXLWorksheet ws, int row, List<TemplateCol> columns, Dictionary<string, object?> values)
    {
        for (var c = 0; c < columns.Count; c++)
        {
            var key = columns[c].Key;
            if (!values.TryGetValue(key, out var value))
                value = null;

            var cell = ws.Cell(row, c + 1);

            if (value is DateTime dt)
            {
                cell.Value = dt;
                cell.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            }
            else
            {
                cell.Value = value?.ToString() ?? "";
            }
        }
    }

    private sealed record TemplateCol(string Key, string Header);

    //import 

    public async Task<ImportStudentResultDto> ImportFromExcelAsync(IRemoteStreamContent file)
    {
        if (file == null)
            throw new UserFriendlyException("No file received.");

        await using var stream = file.GetStream();

        if (!stream.CanRead)
            throw new UserFriendlyException("File stream is not readable.");

        if (stream.CanSeek)
        {
            if (stream.Length == 0)
                throw new UserFriendlyException("Uploaded file is empty.");

            stream.Position = 0;
        }

        return await ImportFromExcelInternalAsync(stream);
    }

    public async Task<ImportStudentResultDto> ImportFromExcelInternalAsync(Stream fileStream)
    {
        var result = new ImportStudentResultDto();

        using var wb = new XLWorkbook(fileStream);

        // Prefer the exact sheet name
        var ws = wb.Worksheets.FirstOrDefault(x => x.Name == "Students")
                 ?? throw new UserFriendlyException("Excel sheet 'Students' not found.");

        const int headerRow = 4;
        const int firstDataRow = 5;

        // Build header map: "Admission No" -> column index, etc.
        var headerMap = BuildHeaderMap(ws, headerRow);

        // Required column keys (match your TemplateCol keys)
        // We map by Header text in template, not by key; so we resolve via helper below.
        int colAdmissionNo = GetCol(headerMap, "Admission No");
        int colFirstName = GetCol(headerMap, "First Name");
        int colLastName = GetCol(headerMap, "Last Name");
        int colDob = GetCol(headerMap, "DOB");
        int colGender = GetCol(headerMap, "Gender");
        int colStreet = GetCol(headerMap, "Street Address");
        int colCity = GetCol(headerMap, "City");
        int colProvince = GetCol(headerMap, "Province");
        int colZip = GetCol(headerMap, "Zip Code");
        int colGradeLevel = GetCol(headerMap, "GradeLevel");
        int colSection = GetCol(headerMap, "Section");
        int colEnroll = GetCol(headerMap, "Enrollment Date");
        int colStatus = GetCol(headerMap, "Status");

        int colEmail = GetCol(headerMap, "Email", required: false);
        int colStreet2 = GetCol(headerMap, "Street Address Line 2", required: false);

        int colPFirst = GetCol(headerMap, "Parent First Name");
        int colPLast = GetCol(headerMap, "Parent Last Name");
        int colPRel = GetCol(headerMap, "Parent Relationship");
        int colPPhone = GetCol(headerMap, "Parent Phone");
        int colPEmail = GetCol(headerMap, "Parent Email", required: false);

        int colECFirst = GetCol(headerMap, "Emergency First Name", required: false);
        int colECLast = GetCol(headerMap, "Emergency Last Name", required: false);
        int colECRel = GetCol(headerMap, "Emergency Relationship", required: false);
        int colECPhone = GetCol(headerMap, "Emergency Phone", required: false);
        int colECEmail = GetCol(headerMap, "Emergency Email", required: false);

        int colTerm = GetCol(headerMap, "Term", required: false);
        int colShift = GetCol(headerMap, "Shift", required: false);

        int colPrevSchool = GetCol(headerMap, "Previous School", required: false);
        int colPrevGrade = GetCol(headerMap, "Previous GradeLevel", required: false);
        int colStudentIdNo = GetCol(headerMap, "Student ID No", required: false);

        int colMedical = GetCol(headerMap, "Medical Conditions", required: false);
        int colExtra = GetCol(headerMap, "Extracurricular", required: false);
        int colComments = GetCol(headerMap, "Comments", required: false);
        int colAccom = GetCol(headerMap, "Accommodations", required: false);

        var lastRowUsed = ws.LastRowUsed()?.RowNumber() ?? headerRow;
        if (lastRowUsed < firstDataRow)
            return result;

        for (int r = firstDataRow; r <= lastRowUsed; r++)
        {
            result.TotalRows++;

            var admissionNo = ReadString(ws, r, colAdmissionNo);
            if (admissionNo.IsNullOrWhiteSpace())
            {
                result.SkippedRows++;
                continue;
            }

            try
            {
                // Read + validate core fields
                var firstName = ReadRequiredString(ws, r, colFirstName, "First Name");
                var lastName = ReadRequiredString(ws, r, colLastName, "Last Name");
                var dob = ReadRequiredDate(ws, r, colDob, "DOB");
                var gender = ReadRequiredEnum<Gender>(ws, r, colGender, "Gender");

                var street = ReadRequiredString(ws, r, colStreet, "Street Address");
                var street2 = ReadString(ws, r, colStreet2);
                var city = ReadRequiredEnum<City>(ws, r, colCity, "City");
                var province = ReadRequiredEnum<Province>(ws, r, colProvince, "Province");
                var zip = ReadRequiredString(ws, r, colZip, "Zip Code");

                var gradeLevel = ReadRequiredEnum<GradeLevel>(ws, r, colGradeLevel, "GradeLevel");
                var section = ReadRequiredEnum<Section>(ws, r, colSection, "Section");
                var enrollment = ReadRequiredDate(ws, r, colEnroll, "Enrollment Date");
                var status = ReadRequiredEnum<Status>(ws, r, colStatus, "Status");

                var email = ReadString(ws, r, colEmail);

                // Parent required
                var pFirst = ReadRequiredString(ws, r, colPFirst, "Parent First Name");
                var pLast = ReadRequiredString(ws, r, colPLast, "Parent Last Name");
                var pRel = ReadRequiredEnum<RelationShipToStudent>(ws, r, colPRel, "Parent Relationship");
                var pPhone = ReadRequiredString(ws, r, colPPhone, "Parent Phone");
                var pEmail = ReadString(ws, r, colPEmail);

                // Emergency optional
                var ecFirst = ReadString(ws, r, colECFirst);
                var ecLast = ReadString(ws, r, colECLast);
                var ecRel = ReadNullableEnum<RelationShipToStudent>(ws, r, colECRel);
                var ecPhone = ReadString(ws, r, colECPhone);
                var ecEmail = ReadString(ws, r, colECEmail);

                // Optional enums (default to your domain defaults if blank)
                var term = ReadNullableEnum<Term>(ws, r, colTerm) ?? Term.Fall;
                var shift = ReadNullableEnum<Shift>(ws, r, colShift) ?? Shift.Morning;

                // Previous background
                var prevSchool = ReadString(ws, r, colPrevSchool);
                var prevGrade = ReadNullableEnum<GradeLevel>(ws, r, colPrevGrade) ?? default; // NOTE: your Create expects GradeLevel pGrade (non-null)
                var studentIdNo = ReadString(ws, r, colStudentIdNo);

                // Additional
                var medical = ReadString(ws, r, colMedical);
                var extracurricular = ReadString(ws, r, colExtra);
                var comments = ReadString(ws, r, colComments);
                var accommodations = ReadString(ws, r, colAccom);

                // IMPORTANT: your CreateAsync signature expects "GradeLevel pGrade" not nullable.
                // If you want it optional, change Create signature; otherwise choose a default.
                // Here: if empty, use current gradeLevel as fallback.
                var previousGradeLevel = prevGrade;
                if (EqualityComparer<GradeLevel>.Default.Equals(previousGradeLevel, default))
                    previousGradeLevel = gradeLevel;

                // Upsert: by AdmissionNo
                var existing = await _studentRepository.FindByAdmissionNoAsync(admissionNo);

                if (existing == null)
                {
                    var created = await _studentManager.CreateAsync(
                        admissionNo,
                        firstName,
                        lastName,
                        gender,
                        NormalizeDate(dob),
                        NormalizeDate(enrollment),
                        gradeLevel,
                        section,
                        term,
                        shift,
                        city,
                        province,
                        street,
                        zip,
                        pFirst,
                        pLast,
                        previousGradeLevel,
                        pRel,
                        pPhone,
                        status,
                        email,
                        street2,
                        pEmail,
                        ecFirst,
                        ecLast,
                        ecRel,
                        ecPhone,
                        ecEmail,
                        prevSchool,
                        studentIdNo,
                        medical,
                        extracurricular,
                        comments,
                        accommodations
                    );

                    await _studentRepository.InsertAsync(created, autoSave: true);
                    result.Created++;
                }
                else
                {
                    // Overwrite all fields with Excel values (this is your "overwrite data" requirement)

                    await _studentManager.ChangeNameAsync(existing, firstName, lastName);
                    await _studentManager.ChangeContactsAsync(existing, email);
                    await _studentManager.ChangeAddressAsync(existing, street, street2, city, province, zip);
                    await _studentManager.ChangeParentInfoAsync(existing, pFirst, pLast, pRel, pPhone, pEmail);
                    await _studentManager.ChangeEmergencyContactAsync(existing, ecFirst, ecLast, ecRel, ecPhone, ecEmail);
                    await _studentManager.ChangeStatusAsync(existing, status);

                    existing.AdmissionNo = admissionNo;
                    existing.Gender = gender;
                    existing.DOB = NormalizeDate(dob);

                    existing.GradeLevel = gradeLevel;
                    existing.Section = section;
                    existing.EnrollmentDate = NormalizeDate(enrollment);
                    existing.Term = term;
                    existing.Shift = shift;
                    existing.Status = status;

                    existing.City = city;
                    existing.Province = province;

                    existing.PerviousSchool = prevSchool;
                    existing.Grade = previousGradeLevel;
                    existing.StudentIdNo = studentIdNo;

                    existing.MedicalConditions = medical;
                    existing.Extracurrucular = extracurricular;
                    existing.Commnets = comments;
                    existing.Accommodations = accommodations;

                    await _studentRepository.UpdateAsync(existing, autoSave: true);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                // Row-level error reporting (do not stop whole import)
                var msg = ex is UserFriendlyException ufe ? ufe.Message : ex.Message;
                result.Errors.Add($"Row {r}: {msg}");
            }
        }

        return result;
    }

    private static DateTime NormalizeDate(DateTime d)
        => new DateTime(d.Year, d.Month, d.Day, 0, 0, 0, DateTimeKind.Unspecified);

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet ws, int headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        var lastCol = ws.Row(headerRow).LastCellUsed()?.Address.ColumnNumber ?? 1;
        for (int c = 1; c <= lastCol; c++)
        {
            var header = ws.Cell(headerRow, c).GetString()?.Trim();
            if (!header.IsNullOrWhiteSpace() && !map.ContainsKey(header))
                map[header] = c;
        }

        return map;
    }

    private static int GetCol(Dictionary<string, int> headerMap, string header, bool required = true)
    {
        if (headerMap.TryGetValue(header, out var col))
            return col;

        if (!required) return -1;

        throw new UserFriendlyException($"Excel template column '{header}' not found in header row.");
    }

    private static string? ReadString(IXLWorksheet ws, int row, int col)
    {
        if (col <= 0) return null;
        return ws.Cell(row, col).GetString()?.Trim();
    }

    private static string ReadRequiredString(IXLWorksheet ws, int row, int col, string fieldName)
    {
        var v = ReadString(ws, row, col);
        if (v.IsNullOrWhiteSpace())
            throw new UserFriendlyException($"{fieldName} is required.");
        return v!;
    }

    private static DateTime ReadRequiredDate(IXLWorksheet ws, int row, int col, string fieldName)
    {
        var cell = ws.Cell(row, col);

        // Excel date cell
        if (cell.DataType == XLDataType.DateTime)
            return cell.GetDateTime();

        // numeric date (Excel serial)
        if (cell.DataType == XLDataType.Number && cell.TryGetValue<double>(out var n))
        {
            // ClosedXML usually handles DateTime type; but in case:
            return DateTime.FromOADate(n);
        }

        // string date
        var s = cell.GetString()?.Trim();
        if (!s.IsNullOrWhiteSpace())
        {
            if (DateTime.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return dt;

            if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                return dt;
        }

        throw new UserFriendlyException($"{fieldName} is invalid. Use yyyy-MM-dd.");
    }

    private static T ReadRequiredEnum<T>(IXLWorksheet ws, int row, int col, string fieldName) where T : struct
    {
        var s = ReadString(ws, row, col);
        if (s.IsNullOrWhiteSpace())
            throw new UserFriendlyException($"{fieldName} is required.");

        if (Enum.TryParse<T>(s, ignoreCase: true, out var e))
            return e;

        throw new UserFriendlyException($"{fieldName} is invalid: '{s}'.");
    }

    private static T? ReadNullableEnum<T>(IXLWorksheet ws, int row, int col) where T : struct
    {
        var s = ReadString(ws, row, col);
        if (s.IsNullOrWhiteSpace())
            return null;

        if (Enum.TryParse<T>(s, ignoreCase: true, out var e))
            return e;

        throw new UserFriendlyException($"Invalid value '{s}'.");
    }



}
