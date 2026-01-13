using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;
using EHub.Students;
using EHub.Staffs;
using EHub.Subjects;
using EHub.Teaching;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using EHub.StudentDocuments;
using EHub.FileAttachments;
using EHub.StaffDocuments;
using EHub.StudentAttendances;

namespace EHub.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class EHubDbContext :
    AbpDbContext<EHubDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Student> Students { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<TeacherSubject> TeacherSubjects { get; set; }

    public DbSet<StudentDocument> StudentDocuments { get; set; }
    public DbSet<StaffDocument> StaffDocuments { get; set; }

    public DbSet<StudentAttendance> StudentAttendances { get; set; }
    #region Entities from the modules

    /* Notice: We only implemented IIdentityProDbContext and ISaasDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityProDbContext and ISaasDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public EHubDbContext(DbContextOptions<EHubDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        /* Configure your own tables/entities inside here */
        builder.Entity<Student>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "Students", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            // --- Basic Info ---
            b.Property(x => x.AdmissionNo)
                .IsRequired()
                .HasMaxLength(StudentConsts.AdmissionNoMaxLength);

            b.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.Email)
                .HasMaxLength(StudentConsts.EmailMaxLength);

            b.Property(x => x.Gender)
                .IsRequired();

            b.Property(x => x.DOB)
                .IsRequired();

            b.Property(x => x.EnrollmentDate)
                .IsRequired();

            b.Property(x => x.Status)
                .IsRequired();

            b.Property(x => x.Term)
                .IsRequired();

            b.Property(x => x.Shift)
                .IsRequired();

            // --- Address ---
            b.Property(x => x.StreetAddress)
                .IsRequired()
                .HasMaxLength(StudentConsts.StreetMaxLength);

            b.Property(x => x.StreetAddressLine2)
                .HasMaxLength(StudentConsts.StreetMaxLength);

            b.Property(x => x.City)
                .IsRequired();

            b.Property(x => x.Province)
                .IsRequired();

            b.Property(x => x.ZipCode)
                .IsRequired()
                .HasMaxLength(StudentConsts.ZipCodeMaxLength);

            // --- Parent / Guardian Info ---
            b.Property(x => x.PFirstName)
                .IsRequired()
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.PLastName)
                .IsRequired()
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.PRelatonShipToStudent)
                .IsRequired();

            b.Property(x => x.PPhone)
                .IsRequired()
                .HasMaxLength(StudentConsts.PhoneMaxLength);

            b.Property(x => x.PEmail)
                .HasMaxLength(StudentConsts.EmailMaxLength);

            // --- Emergency Contact Info ---
            b.Property(x => x.ECFirstName)
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.ECLastName)
                .HasMaxLength(StudentConsts.NameMaxLength);

            b.Property(x => x.ECRelationShipToStudent);

            b.Property(x => x.ECPhone)
                .HasMaxLength(StudentConsts.PhoneMaxLength);

            b.Property(x => x.ECEmail)
                .HasMaxLength(StudentConsts.EmailMaxLength);

            // --- Education ---
            b.Property(x => x.GradeLevel)
                .IsRequired();

            b.Property(x => x.Section)
                .IsRequired();

            // --- Previous Education ---
            b.Property(x => x.PerviousSchool)
                .HasMaxLength(StudentConsts.SchoolNameMaxLength);

            b.Property(x => x.Grade).IsRequired();

            b.Property(x => x.StudentIdNo)
                .HasMaxLength(StudentConsts.StudentIdNoMaxLength);

            // --- Additional Info ---
            b.Property(x => x.MedicalConditions)
                .HasMaxLength(StudentConsts.DescriptionMaxLength);

            b.Property(x => x.Extracurrucular)
                .HasMaxLength(StudentConsts.DescriptionMaxLength);

            b.Property(x => x.Commnets)
                .HasMaxLength(StudentConsts.DescriptionMaxLength);

            b.Property(x => x.Accommodations)
                .HasMaxLength(StudentConsts.DescriptionMaxLength);

            // --- Indexes ---
            b.HasIndex(x => new { x.TenantId, x.AdmissionNo }).IsUnique();
            b.HasIndex(x => x.Email);
        });

        builder.Entity<Staff>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "Staffs", EHubConsts.DbSchema);
            b.ConfigureByConvention(); // Includes TenantId, Auditing, etc.

            // --- Personal Information ---
            b.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(StaffConsts.NameMaxLength);

            b.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(StaffConsts.NameMaxLength);

            b.Property(x => x.PhoneNo)
                .IsRequired()
                .HasMaxLength(StaffConsts.PhoneMaxLength);

            b.Property(x => x.Email)
                .HasMaxLength(StaffConsts.EmailMaxLength);

            b.Property(x => x.DOB)
                .IsRequired();

            b.Property(x => x.Nationality)
                .IsRequired();

            b.Property(x => x.RelationshipStatus)
                .IsRequired();

            b.Property(x => x.Gender)
                .IsRequired();

            b.Property(x => x.LanguageKnown)
                .IsRequired()
                .HasMaxLength(StaffConsts.LanguageKnownMaxLength);

            b.Property(x => x.DisabilityStatus)
                .IsRequired();

            // --- Address Information ---
            b.Property(x => x.StreetAddress)
                .IsRequired()
                .HasMaxLength(StaffConsts.StreetMaxLength);

            b.Property(x => x.StreetAddressLine2)
                .HasMaxLength(StaffConsts.StreetMaxLength);

            b.Property(x => x.City)
                .IsRequired();

            b.Property(x => x.Province)
                .IsRequired();

            b.Property(x => x.ZipCode)
                .IsRequired()
                .HasMaxLength(StaffConsts.ZipCodeMaxLength);

            // --- Job Information ---
            b.Property(x => x.EmployeeCode)
                .IsRequired()
                .HasMaxLength(StaffConsts.EmployeeCodeMaxLength);

            b.Property(x => x.JoiningDate)
                .IsRequired();

            b.Property(x => x.Designation)
                .HasMaxLength(StaffConsts.DesignationMaxLength);

            b.Property(x => x.Department)
                .IsRequired();

            b.Property(x => x.EmploymentType)
                .IsRequired();

            b.Property(x => x.JobStatus)
                .IsRequired();

            b.Property(x => x.Salary)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.ReportingManager)
                .HasMaxLength(StaffConsts.ReportingManagerMaxLength);

            b.Property(x => x.WorkShift);

            b.Property(x => x.ContractStartDate);

            b.Property(x => x.ContractEndDate);

            b.Property(x => x.Remarks)
                .HasMaxLength(StaffConsts.RemarksMaxLength);

            // --- Indexes ---
            b.HasIndex(x => new { x.TenantId, x.EmployeeCode }).IsUnique();
            b.HasIndex(x => x.PhoneNo);
            b.HasIndex(x => x.Email);
            b.HasIndex(x => x.Department);
            b.HasIndex(x => x.JobStatus);
        });

        builder.Entity<Subject>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "Subjects", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(SubjectConsts.CodeMaxLength);

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(SubjectConsts.NameMaxLength);

            b.Property(x => x.ShortName)
                .HasMaxLength(SubjectConsts.ShortNameMaxLength);

            b.Property(x => x.Description)
                .HasMaxLength(SubjectConsts.DescriptionMaxLength);

            b.Property(x => x.GradeLevel);

            b.Property(x => x.CreditHours)
                .HasColumnType("decimal(5,2)");

            b.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();

        });

        builder.Entity<TeacherSubject>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "TeacherSubjects", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StaffId).IsRequired();

            // Converter: List<Guid> <-> "g1,g2,g3"
            var listToString = new ValueConverter<List<Guid>, string>(
                v => string.Join(",", v ?? new List<Guid>()),
                v => (v ?? string.Empty)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(Guid.Parse)
                        .Distinct()
                        .ToList()
            );

            // Comparer so EF tracks list value equality (not reference)
            var listComparer = new ValueComparer<List<Guid>>(
                (a, b) => (a ?? new()).SequenceEqual(b ?? new()),
                v => (v ?? new()).Aggregate(0, (acc, g) => HashCode.Combine(acc, g.GetHashCode())),
                v => v == null ? new List<Guid>() : new List<Guid>(v)
            );

            var subjectIdsProp = b.Property(x => x.SubjectIds);

            subjectIdsProp.HasConversion(listToString);
            subjectIdsProp.Metadata.SetValueComparer(listComparer);
            subjectIdsProp.IsRequired();

            b.Property(x => x.IsPrimaryTeacher)
                .IsRequired()
                .HasDefaultValue(false);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.StaffId, x.Section });

        });

        builder.Entity<StudentDocument>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentDocuments", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId).IsRequired();
            b.Property(x => x.Description).HasMaxLength(StudentDocumentConsts.DescriptionMaxLength).IsRequired(false);
            b.Property(x => x.DocumentType).IsRequired();
            b.Property(x => x.IssueDate).IsRequired(false);
            b.Property(x => x.ExpireDate).IsRequired(false);
            b.Property(x => x.IsVerified).IsRequired();
            b.Property(x => x.TenantId)
                .HasColumnName(nameof(StudentDocument.TenantId))
                .IsRequired(false);

            b.OwnsOne(x => x.FileAttachment, fa =>
            {
                fa.Property(f => f.Name).IsRequired().HasMaxLength(FileAttachmentConsts.MaxNameLength);
                fa.Property(f => f.Path).IsRequired().HasMaxLength(FileAttachmentConsts.MaxPathLength);
                fa.Property(f => f.FileExtension).IsRequired();
                fa.Property(f => f.BlobName).IsRequired();
            });

            b.HasOne(x => x.Student)
             .WithMany(x => x.StudentDocuments)
             .HasForeignKey(x => x.StudentId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.DocumentType);
            b.HasIndex(x => x.IsVerified);
        });
        builder.Entity<StaffDocument>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StaffDocuments", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StaffId).IsRequired();
            b.Property(x => x.Description).HasMaxLength(StudentDocumentConsts.DescriptionMaxLength).IsRequired(false);
            b.Property(x => x.StaffDT).IsRequired();
            b.Property(x => x.IssueDate).IsRequired(false);
            b.Property(x => x.ExpireDate).IsRequired(false);
            b.Property(x => x.IsVerified).IsRequired();
            b.Property(x => x.TenantId)
                .HasColumnName(nameof(StudentDocument.TenantId))
                .IsRequired(false);

            b.OwnsOne(x => x.FileAttachments, fa =>
            {
                fa.Property(f => f.Name).IsRequired().HasMaxLength(FileAttachmentConsts.MaxNameLength);
                fa.Property(f => f.Path).IsRequired().HasMaxLength(FileAttachmentConsts.MaxPathLength);
                fa.Property(f => f.FileExtension).IsRequired();
            });

            b.HasOne(x => x.Staff)
             .WithMany(x => x.StaffDocuments)
             .HasForeignKey(x => x.StaffId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.StaffId);
            b.HasIndex(x => x.StaffDT);
            b.HasIndex(x => x.IsVerified);
        });

        builder.Entity<StudentAttendance>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentAttendances", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.AttendanceDate).IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.Property(x => x.Remarks).HasMaxLength(512);

            b.HasOne(x => x.Students)
                .WithMany(x => x.StudentAttendances)
                    .HasForeignKey(x => x.StudentId)
                        .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.TenantId, x.StudentId, x.AttendanceDate });
        });
    }
}
