using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using EHub.Students;

namespace EHub.Staffs
{
    public class StaffManager : DomainService
    {
        private readonly IStaffRepository _staffRepository;

        public StaffManager(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<Staff> CreateAsync(
            Guid? tenantId,
            string firstName,
            string lastName,
            string phoneNo,
            string? email,
            DateTime dob,
            Nationality nationality,
            RelationshipStatus relationshipStatus,
            Gender gender,
            string languageKnown,
            DisabilityStatus disabilityStatus,
            string streetAddress,
            string? streetAddressLine2,
            City city,
            Province province,
            string zipCode,
            DateTime joiningDate,
            string? designation,
            Department department,
            EmploymentType employmentType,
            JobStatus jobStatus,
            decimal? salary,
            string? reportingManager,
            Shift? workShift,
            DateTime? contractStart,
            DateTime? contractEnd,
            string? remarks
        )
        {
            // 🔹 Generate auto employee code
            var employeeCode = await GenerateUniqueEmployeeCodeAsync(tenantId);

            // 🔹 Validation: Joining date should not be in the future
            if (joiningDate > DateTime.UtcNow.Date)
                throw new BusinessException(EHubDomainErrorCodes.InvalidJoiningDate)
                    .WithData("JoiningDate", joiningDate);

            // 🔹 Validation: Contract range consistency
            if (contractStart.HasValue && contractEnd.HasValue && contractStart > contractEnd)
                throw new BusinessException(EHubDomainErrorCodes.InvalidContractDateRange);

            // 🔹 Validation: Age constraint (at least 18 years old)
            var minAge = DateTime.UtcNow.AddYears(-15);
            if (dob > minAge)
                throw new BusinessException(EHubDomainErrorCodes.InvalidDateOfBirth)
                    .WithData("DOB", dob);

            // 🔹 Create entity
            var staff = new Staff(
                GuidGenerator.Create(),
                tenantId,
                firstName,
                lastName,
                phoneNo,
                email,
                dob,
                nationality,
                relationshipStatus,
                gender,
                languageKnown,
                disabilityStatus,
                streetAddress,
                streetAddressLine2,
                city,
                province,
                zipCode,
                employeeCode,
                joiningDate,
                designation,
                department,
                employmentType,
                jobStatus,
                salary,
                reportingManager,
                workShift,
                contractStart,
                contractEnd,
                remarks
            );

            return await _staffRepository.InsertAsync(staff);
        }

        // 🔹 Auto-generate Staff Code (STAFF-00001 → STAFF-00002 → STAFF-00003)
        private async Task<string> GenerateUniqueEmployeeCodeAsync(Guid? tenantId)
        {
            // Get latest staff for tenant (or globally if multi-tenancy disabled)
            var lastStaff = await _staffRepository.GetLastCreatedStaffAsync(tenantId);

            int nextNumber = 1;

            if (lastStaff != null && !string.IsNullOrWhiteSpace(lastStaff.EmployeeCode))
            {
                var parts = lastStaff.EmployeeCode.Split('-');
                if (parts.Length == 2 && int.TryParse(parts[1], out var currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            // Always 5 digits padded (00001, 00002, etc.)
            return $"STAFF-{nextNumber:00000}";
        }

        public async Task<Staff> UpdatePersonalInfoAsync(
            Staff staff,
            string firstName,
            string lastName,
            string phoneNo,
            string? email,
            DateTime dob,
            Nationality nationality,
            RelationshipStatus relationshipStatus,
            Gender gender,
            string languageKnown,
            DisabilityStatus disabilityStatus)
        {
            staff.ChangePersonalInfo(firstName, lastName, phoneNo, email, dob, nationality,
                relationshipStatus, gender, languageKnown, disabilityStatus);

            return await _staffRepository.UpdateAsync(staff, autoSave: true);
        }

        public async Task<Staff> UpdateAddressAsync(
            Staff staff,
            string street,
            string? street2,
            City city,
            Province province,
            string zip)
        {
            staff.ChangeAddress(street, street2, city, province, zip);
            return await _staffRepository.UpdateAsync(staff, autoSave: true);
        }

        public async Task<Staff> UpdateJobInfoAsync(
            Staff staff,
            string? designation,
            Department department,
            EmploymentType employmentType,
            JobStatus jobStatus,
            decimal? salary,
            string? reportingManager,
            Shift? workShift,
            DateTime? contractStart,
            DateTime? contractEnd,
            string? remarks)
        {
            staff.ChangeJobInfo(designation, department, employmentType, jobStatus, salary,
                reportingManager, workShift, contractStart, contractEnd, remarks);

            return await _staffRepository.UpdateAsync(staff, autoSave: true);
        }
    }
}
