using AutoMapper;
using EHub.FileAttachments;
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

    }
}
