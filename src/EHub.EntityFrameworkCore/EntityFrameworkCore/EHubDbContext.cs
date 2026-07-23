using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.Customers;
using EHub.FeeModule;
using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructureItems;
using EHub.FeeModule.FeeStructures;
using EHub.FeeModule.LateFeePolicies;
using EHub.FeeModule.StudentFeeDiscounts;
using EHub.FeeModule.StudentFeeProfiles;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.FileAttachments;
using EHub.StaffAttendances;
using EHub.StaffDocuments;
using EHub.Staffs;
using EHub.StudentAttendances;
using EHub.StudentDocuments;
using EHub.Students;
using EHub.Subjects;
using EHub.Teaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

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
    public DbSet<StaffAttendance> StaffAttendances { get; set; }
    public DbSet<FeeHead> FeeHeads { get; set; }
    public DbSet<FeeStructure> FeeStructures { get; set; }
    public DbSet<FeeStructureItem> FeeStructureItems { get; set; }
    public DbSet<StudentFeeProfile> StudentFeeProfiles { get; set; }
    public DbSet<StudentFeeDiscount> StudentFeeDiscounts { get; set; }
    public DbSet<LateFeePolicy> lateFeePolicies { get; set; }
    public DbSet<StudentMonthlyFee> StudentMonthlyFees { get; set; }
    public DbSet<StudentMonthlyFeeLine> StudentMonthlyFeeLines { get; set; }

    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<ExpenseEntry> ExpenseEntries { get; set; }
    public DbSet<StaffSalaryPayment> StaffSalaryPayments { get; set; }
    public DbSet<ShopSetting> ShopSettings { get; set; }
    public DbSet<ShopProductCategory> ShopProductCategories { get; set; }
    public DbSet<ShopUnit> ShopUnits { get; set; }
    public DbSet<ShopProduct> ShopProducts { get; set; }
    public DbSet<ShopSupplier> ShopSuppliers { get; set; }
    public DbSet<ShopPurchaseOrder> ShopPurchaseOrders { get; set; }
    public DbSet<ShopPurchaseOrderItem> ShopPurchaseOrderItems { get; set; }
    public DbSet<ShopDocumentSequence> ShopDocumentSequences { get; set; }
    public DbSet<ShopGoodsReceipt> ShopGoodsReceipts { get; set; }
    public DbSet<ShopGoodsReceiptItem> ShopGoodsReceiptItems { get; set; }
    public DbSet<ShopStockTransaction> ShopStockTransactions { get; set; }
    public DbSet<ShopSupplierPayment> ShopSupplierPayments { get; set; }
    public DbSet<ShopSupplierPaymentAllocation> ShopSupplierPaymentAllocations { get; set; }
    public DbSet<ShopPurchaseReturn> ShopPurchaseReturns { get; set; }
    public DbSet<ShopPurchaseReturnItem> ShopPurchaseReturnItems { get; set; }
    public DbSet<ShopCustomer> ShopCustomers { get; set; }
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

            b.Property(x => x.RollNo)
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

        });

        builder.Entity<StaffAttendance>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StaffAttendances", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.AttendanceDate).IsRequired();
            b.Property(x => x.Status).IsRequired();

            b.Property(x => x.Remarks).HasMaxLength(512);

            b.HasOne(x => x.Staff)
                .WithMany(x => x.StaffAttendances)
                    .HasForeignKey(x => x.StaffId)
                        .OnDelete(DeleteBehavior.Cascade);

        });

        builder.Entity<FeeHead>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "FeeHeads", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(FeeModuleConsts.NameMaxLength);

            b.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        builder.Entity<FeeStructure>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "FeeStructures", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.GradeLevel)
                .IsRequired();

            b.Property(x => x.Shift)
                .IsRequired();

            b.Property(x => x.Term)
                .IsRequired();

            b.Property(x => x.EffectiveFrom)
                .IsRequired();

            b.Property(x => x.EffectiveTo);

            b.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });

        builder.Entity<FeeStructureItem>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "FeeStructureItems", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.FeeStructureId).IsRequired();
            b.Property(x => x.FeeHeadId).IsRequired();

            b.Property(x => x.MonthlyAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.IsMandatory)
                .IsRequired()
                .HasDefaultValue(false);

            b.HasOne(x => x.FeeStructure)
                .WithMany()
                .HasForeignKey(x => x.FeeStructureId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.FeeHead)
                .WithMany()
                .HasForeignKey(x => x.FeeHeadId)
                .IsRequired()
                .OnDelete(DeleteBehavior.NoAction);

        });
        builder.Entity<StudentFeeProfile>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentFeeProfiles", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId).IsRequired();
            b.Property(x => x.FeeStructureId).IsRequired();

            b.Property(x => x.EffectiveFrom).IsRequired();
            b.Property(x => x.EffectiveTo);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.FeeStructure)
                .WithMany()
                .HasForeignKey(x => x.FeeStructureId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<StudentFeeDiscount>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentFeeDiscounts", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId).IsRequired();
            b.Property(x => x.FeeHeadId); // nullable
            b.Property(x => x.DiscountType).IsRequired();
            b.Property(x => x.Value).IsRequired().HasPrecision(18, 2);
            b.Property(x => x.StartMonth); // nullable
            b.Property(x => x.EndMonth); // nullable
            b.Property(x => x.Reason).IsRequired().HasMaxLength(500);
            b.Property(x => x.ApprovedByStaffId); // nullable
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            // Foreign Keys
            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.FeeHead)
                .WithMany()
                .HasForeignKey(x => x.FeeHeadId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ApprovedByStaff)
                .WithMany()
                .HasForeignKey(x => x.ApprovedByStaffId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<LateFeePolicy>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "LateFeePolicies", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.GradeLevel); // nullable enum
            b.Property(x => x.Section); // nullable enum
            b.Property(x => x.Shift); // nullable enum
            b.Property(x => x.Term); // nullable enum
            b.Property(x => x.GraceDays).IsRequired();
            b.Property(x => x.Type).IsRequired();
            b.Property(x => x.Value).IsRequired().HasPrecision(18, 2);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        });
        builder.Entity<StudentMonthlyFee>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentMonthlyFees", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentId).IsRequired();
            b.Property(x => x.Month).IsRequired();
            b.Property(x => x.DueDate);
            b.Property(x => x.Remarks).HasMaxLength(1024);

            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        builder.Entity<StudentMonthlyFeeLine>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StudentMonthlyFeeLines", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StudentMonthlyFeeId).IsRequired();
            b.Property(x => x.FeeHeadId).IsRequired();

            b.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
            b.Property(x => x.AdjustmentAmount).HasPrecision(18, 2);
            b.Property(x => x.LateFeeAmount).HasPrecision(18, 2);
            b.Property(x => x.PaidAmount).HasPrecision(18, 2);

            b.Property(x => x.NetAmount).HasPrecision(18, 2);
            b.Property(x => x.OutstandingAmount).HasPrecision(18, 2);


            b.HasOne(x => x.StudentMonthlyFee)
                .WithMany() // or .WithMany(x => x.Lines) if you add navigation on StudentMonthlyFee
                .HasForeignKey(x => x.StudentMonthlyFeeId)
                .OnDelete(DeleteBehavior.NoAction);

            b.HasOne(x => x.FeeHead)
                .WithMany()
                .HasForeignKey(x => x.FeeHeadId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        builder.Entity<ExpenseCategory>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "ExpenseCategories", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);

            b.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            b.Property(x => x.EntryLimitType).IsRequired(false);
        });

        builder.Entity<ShopSetting>(b =>
        {
            b.ToTable("ShopSettings", EHubConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.HasIndex(x => x.TenantId).IsUnique();
            b.Property(x => x.ShopDisplayName).IsRequired().HasMaxLength(ShopSettingConsts.ShopDisplayNameMaxLength);
            b.Property(x => x.Phone).HasMaxLength(ShopSettingConsts.PhoneMaxLength);
            b.Property(x => x.AlternatePhone).HasMaxLength(ShopSettingConsts.PhoneMaxLength);
            b.Property(x => x.Email).HasMaxLength(ShopSettingConsts.EmailMaxLength);
            b.Property(x => x.Website).HasMaxLength(ShopSettingConsts.WebsiteMaxLength);
            b.Property(x => x.AddressLine1).HasMaxLength(ShopSettingConsts.AddressMaxLength);
            b.Property(x => x.AddressLine2).HasMaxLength(ShopSettingConsts.AddressMaxLength);
            b.Property(x => x.City).HasMaxLength(ShopSettingConsts.LocationMaxLength);
            b.Property(x => x.StateOrProvince).HasMaxLength(ShopSettingConsts.LocationMaxLength);
            b.Property(x => x.PostalCode).HasMaxLength(ShopSettingConsts.PostalCodeMaxLength);
            b.Property(x => x.Country).HasMaxLength(ShopSettingConsts.LocationMaxLength);
            b.Property(x => x.CurrencyCode).IsRequired().HasMaxLength(ShopSettingConsts.CurrencyCodeMaxLength).HasDefaultValue("PKR");
            b.Property(x => x.CurrencySymbol).IsRequired().HasMaxLength(ShopSettingConsts.CurrencySymbolMaxLength).HasDefaultValue("₨");
            b.Property(x => x.TaxNumber).HasMaxLength(ShopSettingConsts.TaxNumberMaxLength);
            b.Property(x => x.DefaultTaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.InvoicePrefix).IsRequired().HasMaxLength(ShopSettingConsts.PrefixMaxLength).HasDefaultValue("INV");
            b.Property(x => x.PurchaseOrderPrefix).IsRequired().HasMaxLength(ShopSettingConsts.PrefixMaxLength).HasDefaultValue("PO");
            b.Property(x => x.ReceiptFooter).HasMaxLength(ShopSettingConsts.LongTextMaxLength);
            b.Property(x => x.ReturnPolicy).HasMaxLength(ShopSettingConsts.LongTextMaxLength);
            b.Property(x => x.AllowNegativeStock).HasDefaultValue(false);
            b.Property(x => x.AutoGenerateProductBarcode).HasDefaultValue(true);
            b.Property(x => x.AutoGenerateInvoiceQrCode).HasDefaultValue(true);
            b.Property(x => x.DefaultLowStockLevel).HasPrecision(18, 2).HasDefaultValue(5);
            b.Property(x => x.DecimalPlaces).HasDefaultValue(2);
            b.Property(x => x.IsConfigured).HasDefaultValue(false);
        });

        builder.Entity<ShopProductCategory>(b =>
        {
            b.ToTable("ShopProductCategories", EHubConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopProductCategoryConsts.NameMaxLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopProductCategoryConsts.CodeMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopProductCategoryConsts.DescriptionMaxLength);
            b.Property(x => x.DisplayOrder).IsRequired().HasDefaultValue(0);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            b.HasOne(x => x.ParentCategory).WithMany(x => x.ChildCategories)
                .HasForeignKey(x => x.ParentCategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ParentCategoryId });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasIndex(x => new { x.TenantId, x.ParentCategoryId, x.Name })
                .IsUnique().HasFilter("[ParentCategoryId] IS NOT NULL");
            b.HasIndex(x => new { x.TenantId, x.Name })
                .IsUnique().HasFilter("[ParentCategoryId] IS NULL");
        });

        builder.Entity<ShopUnit>(b =>
        {
            b.ToTable("ShopUnits", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopUnitConsts.NameMaxLength);
            b.Property(x => x.ShortName).IsRequired().HasMaxLength(ShopUnitConsts.ShortNameMaxLength);
            b.Property(x => x.AllowDecimal).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ShortName }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<ShopProduct>(b =>
        {
            b.ToTable("ShopProducts", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.CategoryId).IsRequired();
            b.Property(x => x.UnitId).IsRequired();
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopProductConsts.NameMaxLength);
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopProductConsts.CodeMaxLength);
            b.Property(x => x.SKU).HasMaxLength(ShopProductConsts.SkuMaxLength);
            b.Property(x => x.Barcode).HasMaxLength(ShopProductConsts.BarcodeMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopProductConsts.DescriptionMaxLength);
            b.Property(x => x.Brand).HasMaxLength(ShopProductConsts.BrandMaxLength);
            b.Property(x => x.Model).HasMaxLength(ShopProductConsts.ModelMaxLength);

            b.Property(x => x.PurchasePrice).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.SalePrice).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.WholesalePrice).HasPrecision(18, 4);
            b.Property(x => x.MinimumSalePrice).HasPrecision(18, 4);
            b.Property(x => x.TaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);

            b.Property(x => x.CurrentStock).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.MinimumStockLevel).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.MaximumStockLevel).HasPrecision(18, 4);
            b.Property(x => x.ReorderLevel).HasPrecision(18, 4).HasDefaultValue(0);

            b.Property(x => x.TrackBatch).IsRequired().HasDefaultValue(false);
            b.Property(x => x.TrackExpiry).IsRequired().HasDefaultValue(false);
            b.Property(x => x.TrackSerialNumber).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsTaxable).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Unit).WithMany().HasForeignKey(x => x.UnitId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.CategoryId });
            b.HasIndex(x => new { x.TenantId, x.UnitId });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasIndex(x => new { x.TenantId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.CurrentStock });
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.SKU }).IsUnique().HasFilter("[SKU] IS NOT NULL");
            b.HasIndex(x => new { x.TenantId, x.Barcode }).IsUnique().HasFilter("[Barcode] IS NOT NULL");
        });

        builder.Entity<ShopSupplier>(b =>
        {
            b.ToTable("ShopSuppliers", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopSupplierConsts.CodeMaxLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopSupplierConsts.NameMaxLength);
            b.Property(x => x.ContactPerson).HasMaxLength(ShopSupplierConsts.ContactPersonMaxLength);
            b.Property(x => x.Phone).HasMaxLength(ShopSupplierConsts.PhoneMaxLength);
            b.Property(x => x.AlternatePhone).HasMaxLength(ShopSupplierConsts.PhoneMaxLength);
            b.Property(x => x.Email).HasMaxLength(ShopSupplierConsts.EmailMaxLength);
            b.Property(x => x.AddressLine1).HasMaxLength(ShopSupplierConsts.AddressLineMaxLength);
            b.Property(x => x.AddressLine2).HasMaxLength(ShopSupplierConsts.AddressLineMaxLength);
            b.Property(x => x.City).HasMaxLength(ShopSupplierConsts.CityMaxLength);
            b.Property(x => x.StateOrProvince).HasMaxLength(ShopSupplierConsts.StateOrProvinceMaxLength);
            b.Property(x => x.PostalCode).HasMaxLength(ShopSupplierConsts.PostalCodeMaxLength);
            b.Property(x => x.Country).HasMaxLength(ShopSupplierConsts.CountryMaxLength);
            b.Property(x => x.TaxNumber).HasMaxLength(ShopSupplierConsts.TaxNumberMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopSupplierConsts.NotesMaxLength);
            b.Property(x => x.OpeningBalance).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CreditLimit).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.PaymentTermsDays).IsRequired().HasDefaultValue(0);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasIndex(x => new { x.TenantId, x.City });
            b.HasIndex(x => new { x.TenantId, x.Country });
            b.HasIndex(x => new { x.TenantId, x.Phone });
            b.HasIndex(x => new { x.TenantId, x.Email });
            b.HasIndex(x => new { x.TenantId, x.TaxNumber });
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
        });

        builder.Entity<ShopCustomer>(b =>
        {
            b.ToTable("ShopCustomers", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopCustomerConsts.CodeMaxLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopCustomerConsts.NameMaxLength);
            b.Property(x => x.CustomerType).IsRequired().HasConversion<int>().HasDefaultValue(ShopCustomerType.Individual);
            b.Property(x => x.ContactPerson).HasMaxLength(ShopCustomerConsts.ContactPersonMaxLength);
            b.Property(x => x.Phone).HasMaxLength(ShopCustomerConsts.PhoneMaxLength);
            b.Property(x => x.AlternatePhone).HasMaxLength(ShopCustomerConsts.PhoneMaxLength);
            b.Property(x => x.Email).HasMaxLength(ShopCustomerConsts.EmailMaxLength);
            b.Property(x => x.AddressLine1).HasMaxLength(ShopCustomerConsts.AddressLineMaxLength);
            b.Property(x => x.AddressLine2).HasMaxLength(ShopCustomerConsts.AddressLineMaxLength);
            b.Property(x => x.City).HasMaxLength(ShopCustomerConsts.CityMaxLength);
            b.Property(x => x.StateOrProvince).HasMaxLength(ShopCustomerConsts.StateOrProvinceMaxLength);
            b.Property(x => x.PostalCode).HasMaxLength(ShopCustomerConsts.PostalCodeMaxLength);
            b.Property(x => x.Country).HasMaxLength(ShopCustomerConsts.CountryMaxLength);
            b.Property(x => x.TaxNumber).HasMaxLength(ShopCustomerConsts.TaxNumberMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopCustomerConsts.NotesMaxLength);
            b.Property(x => x.OpeningBalance).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CreditLimit).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.PaymentTermsDays).IsRequired().HasDefaultValue(0);
            b.Property(x => x.IsWalkInCustomer).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.CustomerType });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
            b.HasIndex(x => new { x.TenantId, x.IsWalkInCustomer });
            b.HasIndex(x => new { x.TenantId, x.Phone });
            b.HasIndex(x => new { x.TenantId, x.City });
            b.HasIndex(x => new { x.TenantId, x.Country });
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
        });

        builder.Entity<ShopPurchaseOrder>(b =>
        {
            b.ToTable("ShopPurchaseOrders", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.PurchaseOrderNumber).IsRequired().HasMaxLength(ShopPurchaseOrderConsts.PurchaseOrderNumberMaxLength);
            b.Property(x => x.SupplierId).IsRequired();
            b.Property(x => x.OrderDate).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopPurchaseOrderStatus.Draft);
            b.Property(x => x.SupplierReference).HasMaxLength(ShopPurchaseOrderConsts.SupplierReferenceMaxLength);
            b.Property(x => x.SubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ShippingCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.OtherCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.GrandTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.Notes).HasMaxLength(ShopPurchaseOrderConsts.NotesMaxLength);
            b.Property(x => x.RejectionReason).HasMaxLength(ShopPurchaseOrderConsts.RejectionReasonMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopPurchaseOrderConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.PurchaseOrder).HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SupplierId });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.OrderDate });
            b.HasIndex(x => new { x.TenantId, x.ExpectedDeliveryDate });
            b.HasIndex(x => new { x.TenantId, x.PurchaseOrderNumber }).IsUnique();
        });

        builder.Entity<ShopPurchaseOrderItem>(b =>
        {
            b.ToTable("ShopPurchaseOrderItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.PurchaseOrderId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopPurchaseOrderConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopPurchaseOrderConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopPurchaseOrderConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopPurchaseOrderConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopPurchaseOrderConsts.DescriptionMaxLength);
            b.Property(x => x.OrderedQuantity).HasPrecision(18, 4);
            b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.UnitPurchasePrice).HasPrecision(18, 2);
            b.Property(x => x.DiscountPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineSubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineTotal).HasPrecision(18, 2).HasDefaultValue(0);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.PurchaseOrderId, x.ProductId }).IsUnique();
        });

        builder.Entity<ShopDocumentSequence>(b =>
        {
            b.ToTable("ShopDocumentSequences", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.DocumentType).IsRequired().HasMaxLength(64);
            b.Property(x => x.LastNumber).IsRequired().HasDefaultValue(0);
            b.HasIndex(x => new { x.TenantId, x.DocumentType }).IsUnique();
        });

        builder.Entity<ShopGoodsReceipt>(b =>
        {
            b.ToTable("ShopGoodsReceipts", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.GoodsReceiptNumber).IsRequired().HasMaxLength(ShopGoodsReceiptConsts.GoodsReceiptNumberMaxLength);
            b.Property(x => x.PurchaseOrderId).IsRequired();
            b.Property(x => x.SupplierId).IsRequired();
            b.Property(x => x.SupplierInvoiceNumber).HasMaxLength(ShopGoodsReceiptConsts.SupplierInvoiceNumberMaxLength);
            b.Property(x => x.ReceiptDate).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopGoodsReceiptStatus.Draft);
            b.Property(x => x.SubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ShippingCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.OtherCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.GrandTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.Notes).HasMaxLength(ShopGoodsReceiptConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopGoodsReceiptConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.PurchaseOrder).WithMany().HasForeignKey(x => x.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.GoodsReceipt).HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.PurchaseOrderId });
            b.HasIndex(x => new { x.TenantId, x.SupplierId });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.ReceiptDate });
            b.HasIndex(x => new { x.TenantId, x.SupplierInvoiceNumber });
            b.HasIndex(x => new { x.TenantId, x.GoodsReceiptNumber }).IsUnique();
        });

        builder.Entity<ShopGoodsReceiptItem>(b =>
        {
            b.ToTable("ShopGoodsReceiptItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.GoodsReceiptId).IsRequired();
            b.Property(x => x.PurchaseOrderItemId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopGoodsReceiptConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopGoodsReceiptConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopGoodsReceiptConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopGoodsReceiptConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.OrderedQuantitySnapshot).HasPrecision(18, 4);
            b.Property(x => x.PreviouslyReceivedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4);
            b.Property(x => x.BonusQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.PurchasePrice).HasPrecision(18, 2);
            b.Property(x => x.SalePrice).HasPrecision(18, 2);
            b.Property(x => x.BatchNumber).HasMaxLength(ShopGoodsReceiptConsts.BatchNumberMaxLength);
            b.Property(x => x.DiscountPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineSubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineTotal).HasPrecision(18, 2).HasDefaultValue(0);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.PurchaseOrderItem).WithMany().HasForeignKey(x => x.PurchaseOrderItemId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.GoodsReceiptId });
            b.HasIndex(x => new { x.TenantId, x.PurchaseOrderItemId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.GoodsReceiptId, x.PurchaseOrderItemId }).IsUnique();
        });

        builder.Entity<ShopPurchaseReturn>(b =>
        {
            b.ToTable("ShopPurchaseReturns", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired(); b.Property(x => x.PurchaseReturnNumber).IsRequired().HasMaxLength(ShopPurchaseReturnConsts.NumberMaxLength);
            b.Property(x => x.Status).HasConversion<int>(); b.Property(x => x.Reason).HasConversion<int>(); b.Property(x => x.ReasonDetails).HasMaxLength(ShopPurchaseReturnConsts.ReasonDetailsMaxLength);
            b.Property(x => x.SubTotal).HasPrecision(18,2); b.Property(x => x.TaxAmount).HasPrecision(18,2); b.Property(x => x.OtherCharges).HasPrecision(18,2); b.Property(x => x.GrandTotal).HasPrecision(18,2);
            b.Property(x => x.Notes).HasMaxLength(ShopPurchaseReturnConsts.NotesMaxLength); b.Property(x => x.CancellationReason).HasMaxLength(ShopPurchaseReturnConsts.CancellationReasonMaxLength);
            b.HasOne(x=>x.Supplier).WithMany().HasForeignKey(x=>x.SupplierId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.GoodsReceipt).WithMany().HasForeignKey(x=>x.GoodsReceiptId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x=>x.Items).WithOne(x=>x.PurchaseReturn).HasForeignKey(x=>x.PurchaseReturnId).OnDelete(DeleteBehavior.Cascade);
            b.HasIndex(x=>new{x.TenantId,x.PurchaseReturnNumber}).IsUnique(); b.HasIndex(x=>new{x.TenantId,x.SupplierId}); b.HasIndex(x=>new{x.TenantId,x.GoodsReceiptId}); b.HasIndex(x=>new{x.TenantId,x.Status}); b.HasIndex(x=>new{x.TenantId,x.ReturnDate});
        });
        builder.Entity<ShopPurchaseReturnItem>(b =>
        {
            b.ToTable("ShopPurchaseReturnItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x=>x.Id); b.Property(x=>x.TenantId).IsRequired();
            b.Property(x=>x.ProductNameSnapshot).HasMaxLength(ShopPurchaseReturnConsts.SnapshotMaxLength); b.Property(x=>x.ProductCodeSnapshot).HasMaxLength(ShopPurchaseReturnConsts.CodeMaxLength); b.Property(x=>x.UnitNameSnapshot).HasMaxLength(ShopPurchaseReturnConsts.UnitMaxLength); b.Property(x=>x.UnitShortNameSnapshot).HasMaxLength(ShopPurchaseReturnConsts.UnitMaxLength); b.Property(x=>x.BatchNumber).HasMaxLength(ShopPurchaseReturnConsts.BatchMaxLength); b.Property(x=>x.Notes).HasMaxLength(ShopPurchaseReturnConsts.NotesMaxLength);
            b.Property(x=>x.ReceivedQuantitySnapshot).HasPrecision(18,4); b.Property(x=>x.PreviouslyReturnedQuantity).HasPrecision(18,4); b.Property(x=>x.ReturnQuantity).HasPrecision(18,4); b.Property(x=>x.UnitPurchasePrice).HasPrecision(18,2); b.Property(x=>x.TaxPercentage).HasPrecision(5,2); b.Property(x=>x.TaxAmount).HasPrecision(18,2); b.Property(x=>x.LineSubTotal).HasPrecision(18,2); b.Property(x=>x.LineTotal).HasPrecision(18,2);
            b.HasOne(x=>x.GoodsReceiptItem).WithMany().HasForeignKey(x=>x.GoodsReceiptItemId).OnDelete(DeleteBehavior.Restrict); b.HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict); b.HasIndex(x=>new{x.PurchaseReturnId,x.GoodsReceiptItemId}).IsUnique();
        });

        builder.Entity<ShopStockTransaction>(b =>
        {
            b.ToTable("ShopStockTransactions", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.TransactionType).IsRequired().HasConversion<int>();
            b.Property(x => x.ReferenceType).IsRequired().HasConversion<int>();
            b.Property(x => x.ReferenceId).IsRequired();
            b.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(ShopStockTransactionConsts.ReferenceNumberMaxLength);
            b.Property(x => x.TransactionDate).IsRequired();
            b.Property(x => x.QuantityIn).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.QuantityOut).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.BalanceQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.UnitCost).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TotalCost).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.BatchNumber).HasMaxLength(ShopStockTransactionConsts.BatchNumberMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopStockTransactionConsts.NotesMaxLength);
            b.Property(x => x.CreationTime).IsRequired();

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.TransactionType });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId });
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.SourceItemId }).IsUnique();
        });

        builder.Entity<ShopSupplierPayment>(b =>
        {
            b.ToTable("ShopSupplierPayments", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.PaymentNumber).IsRequired().HasMaxLength(ShopSupplierPaymentConsts.PaymentNumberMaxLength);
            b.Property(x => x.SupplierId).IsRequired();
            b.Property(x => x.PaymentDate).IsRequired();
            b.Property(x => x.PaymentType).IsRequired().HasConversion<int>();
            b.Property(x => x.PaymentMethod).IsRequired().HasConversion<int>();
            b.Property(x => x.Amount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopSupplierPaymentConsts.ReferenceNumberMaxLength);
            b.Property(x => x.ChequeNumber).HasMaxLength(ShopSupplierPaymentConsts.ChequeNumberMaxLength);
            b.Property(x => x.BankName).HasMaxLength(ShopSupplierPaymentConsts.BankNameMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopSupplierPaymentConsts.NotesMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopSupplierPaymentStatus.Draft);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopSupplierPaymentConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Allocations).WithOne(x => x.SupplierPayment).HasForeignKey(x => x.SupplierPaymentId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SupplierId });
            b.HasIndex(x => new { x.TenantId, x.PaymentDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.PaymentNumber }).IsUnique();
        });

        builder.Entity<ShopSupplierPaymentAllocation>(b =>
        {
            b.ToTable("ShopSupplierPaymentAllocations", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SupplierPaymentId).IsRequired();
            b.Property(x => x.GoodsReceiptId).IsRequired();
            b.Property(x => x.AllocatedAmount).HasPrecision(18, 2).HasDefaultValue(0);

            b.HasOne(x => x.GoodsReceipt).WithMany().HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SupplierPaymentId });
            b.HasIndex(x => new { x.TenantId, x.GoodsReceiptId });
            b.HasIndex(x => new { x.SupplierPaymentId, x.GoodsReceiptId }).IsUnique();
        });

        builder.Entity<ExpenseEntry>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "ExpenseEntries", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.ExpenseDate)
                .IsRequired();

            b.Property(x => x.ExpenseCategoryId)
                .IsRequired();

            b.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(128);

            b.Property(x => x.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            b.Property(x => x.PaidTo)
                .HasMaxLength(128);

            b.Property(x => x.Remarks)
                .HasMaxLength(1024);

            b.HasOne(x => x.ExpenseCategory)
                .WithMany()
                .HasForeignKey(x => x.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<StaffSalaryPayment>(b =>
        {
            b.ToTable(EHubConsts.DbTablePrefix + "StaffSalaryPayments", EHubConsts.DbSchema);
            b.ConfigureByConvention();

            b.Property(x => x.StaffId).IsRequired();
            b.Property(x => x.SalaryMonth).IsRequired();
            b.Property(x => x.PaymentDate).IsRequired();

            b.Property(x => x.SalaryAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            b.Property(x => x.Remarks)
                .HasMaxLength(1024);

            b.HasOne<Staff>()
                .WithMany()
                .HasForeignKey(x => x.StaffId)
                .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
