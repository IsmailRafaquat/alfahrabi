using AutoMapper;
using EHub.ShopManagement.Settings;
using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructureItems;
using EHub.FeeModule.FeeStructures;
using EHub.FeeModule.LateFeePolicies;
using EHub.FeeModule.StudentFeeDiscounts;
using EHub.FeeModule.StudentFeeProfiles;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.FileAttachments;
using EHub.Reports.ExpenseReport;
using EHub.Reports.SalaryReport;
using EHub.StaffAttendances;
using EHub.StaffDocuments;
using EHub.Staffs;
using EHub.StudentAttendances;
using EHub.StudentDocuments;
using EHub.Students;
using EHub.Subjects;

namespace EHub;

public class EHubApplicationAutoMapperProfile : Profile
{
    public EHubApplicationAutoMapperProfile()
    {
        CreateMap<ShopSetting, ShopSettingDto>();
        CreateMap<CreateUpdateShopSettingDto, ShopSetting>();
        CreateMap<Student, StudentDto>()
            .ForMember(x => x.StudentDocument, opt => opt.MapFrom(src => src.StudentDocuments));
        CreateMap<Staff, StaffDto>()
            .ForMember(x => x.StaffDocuments, opt => opt.MapFrom(src => src.StaffDocuments));

        CreateMap<Subject, SubjectDto>();

        CreateMap<StudentDocument, StudentDocumentDto>()
            .ForMember(x => x.FileAttachment, opt => opt.MapFrom(src => src.FileAttachment));
        CreateMap<StaffDocument, StaffDocumentDto>()
            .ForMember(x => x.FileAttachments, opt => opt.MapFrom(src => src.FileAttachments));

        CreateMap<FileAttachment, FileAttachmentDto>();

        CreateMap<StudentAttendance, StudentAttendanceDto>()
            .ForMember(x => x.StudentName, opt => opt.MapFrom(src => src.Students.FirstName + ' ' + src.Students.LastName))
            .ForMember(x => x.AdmissionNo, opt => opt.MapFrom(src => src.Students.AdmissionNo));

        CreateMap<StaffAttendance, StaffAttendanceDto>()
       .ForMember(x => x.StaffName, opt => opt.MapFrom(src => src.Staff.FirstName + ' ' + src.Staff.LastName))
       .ForMember(x => x.EmployeeCode, opt => opt.MapFrom(src => src.Staff.EmployeeCode));

        CreateMap<FeeHead, FeeHeadDto>();
        CreateMap<FeeStructure, FeeStructureDto>();
        CreateMap<FeeStructureItem, FeeStructureItemDto>();
        CreateMap<StudentFeeProfile, StudentFeeProfileDto>();

        CreateMap<StudentFeeDiscount, StudentFeeDiscountDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : string.Empty))
            .ForMember(dest => dest.FeeHeadName, opt => opt.MapFrom(src =>
                src.FeeHead != null ? src.FeeHead.Name : "Total Fee"))
            .ForMember(dest => dest.ApprovedByStaffName, opt => opt.MapFrom(src =>
                src.ApprovedByStaff != null ? $"{src.ApprovedByStaff.FirstName} {src.ApprovedByStaff.LastName}" : null));

        CreateMap<LateFeePolicy, LateFeePolicyDto>()
           .ForMember(dest => dest.IsGlobal, opt => opt.MapFrom(src =>
               !src.GradeLevel.HasValue &&
               !src.Section.HasValue &&
               !src.Shift.HasValue &&
               !src.Term.HasValue));

        CreateMap<StudentMonthlyFee, StudentMonthlyFeeDto>()
            .ForMember(d => d.StudentName, opt => opt.MapFrom(src =>
                src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : null))
            .ForMember(d => d.AdmissionNo, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.AdmissionNo : null))
            .ForMember(d => d.GradeLevel, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.GradeLevel : (GradeLevel?)null));

        CreateMap<StudentMonthlyFeeLine, StudentMonthlyFeeLineDto>()
            .ForMember(d => d.FeeHeadName, opt => opt.MapFrom(src => src.FeeHead != null ? src.FeeHead.Name : null));

        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        CreateMap<ExpenseEntry, ExpenseEntryDto>();
        CreateMap<StaffSalaryPayment, StaffSalaryPaymentDto>();
        CreateMap<ExpenseReport, ExpenseReportDto>();
        CreateMap<StaffSalaryReport, StaffSalaryReportDto>();
    }
}
