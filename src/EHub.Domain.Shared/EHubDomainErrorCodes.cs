namespace EHub;

public static class EHubDomainErrorCodes
{
    public const string StudentAlreadyExists = "StudentAlreadyExists";
    public const string EnrollmentAlreadyExists = "EnrollmentAlreadyExists";
    public const string StudentEmailAlreadyUsed = "StudentEmailAlreadyUsed";
    public const string InvalidDateOfBirth = "InvalidDateOfBirth";
    public const string InvalidEnrollmentDate = "InvalidEnrollmentDate";
    public const string AdmissionNumberGenerationFailed = "EHub:AdmissionNumberGenerationFailed";
    public const string DuplicateEmployeeCode = "EHub:00011";
    public const string InvalidJoiningDate = "EHub:00012";
    public const string InvalidContractDateRange = "EHub:00013";
    public const string InvalidCreditHours = "InvalidCreditHours";
    public const string DuplicateSubject = "DuplicateSubject";
    public const string TeacherSubjectAlreadyExists = "TeacherSubjectAlreadyExists";
    public const string StudentDocumentExpireDateBeforeIssueDate = "EHub:StudentDocument:ExpireDateBeforeIssueDate";
    public const string StudentDocumentEmpty = "EHub:StudentDocumentEmpty";
    public const string StaffDocumentEmpty = "EHub:StaffDocumentEmpty";
    public const string StaffDocumentInvalidDates = "EHub:StaffDocument:StaffDocumentInvalidDates";

    //File Manager
    public const string EmptyFile = "EmptyFileError:00004";
    public const string InvalidFileFormat = "InvalidFileFormatError:00005";
    public const string NullField = "NullField:00006";
}
