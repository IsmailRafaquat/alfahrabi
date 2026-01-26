namespace EHub.FeeModule.StudentMonthlyFeeLines;

public class BulkGenerateStudentMonthlyFeeResultDto
{
    public int TotalStudents { get; set; }
    public int Created { get; set; }
    public int Skipped { get; set; }

    public int LinesCreated { get; set; }
    public int LinesSkipped { get; set; }

    public int MissingFeeProfile { get; set; }
}
