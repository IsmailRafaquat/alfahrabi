using Volo.Abp.Application.Dtos;

namespace EHub.Students;

public class GenerateStudentImportTemplateDto : EntityDto
{
    public GradeLevel GradeLevel { get; set; }
    public Section Section { get; set; }

    /// <summary>
    /// How many empty rows to append after existing students.
    /// </summary>
    public int ExtraEmptyRows { get; set; } = 30;

    /// <summary>
    /// If true, includes existing students for selected class/section as reference/edit.
    /// If false, generates only blank rows.
    /// </summary>
    public bool IncludeExistingStudents { get; set; } = true;
}
