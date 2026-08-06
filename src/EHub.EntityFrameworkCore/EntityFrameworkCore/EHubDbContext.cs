using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.PrintSettings;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.StockAdjustments;
using EHub.ShopManagement.StockCounts;
using EHub.ShopManagement.ProductBatches;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.PurchaseReturns;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.Sales;
using EHub.ShopManagement.SaleReturns;
using EHub.ShopManagement.CustomerPayments;
using EHub.ShopManagement.ExpenseCategories;
using EHub.ShopManagement.Expenses;
using EHub.ShopManagement.CashRegisters;
using EHub.ShopManagement.BankAccounts;
using EHub.ShopManagement.AiAssistant;
using EHub.ShopManagement.Notifications;
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
    public DbSet<ShopPrintSetting> ShopPrintSettings { get; set; }
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
    public DbSet<ShopStockAdjustment> ShopStockAdjustments { get; set; }
    public DbSet<ShopStockAdjustmentItem> ShopStockAdjustmentItems { get; set; }
    public DbSet<ShopStockCount> ShopStockCounts { get; set; }
    public DbSet<ShopStockCountItem> ShopStockCountItems { get; set; }
    public DbSet<ShopProductBatch> ShopProductBatches { get; set; }
    public DbSet<ShopSaleItemBatchAllocation> ShopSaleItemBatchAllocations { get; set; }
    public DbSet<ShopSupplierPayment> ShopSupplierPayments { get; set; }
    public DbSet<ShopSupplierPaymentAllocation> ShopSupplierPaymentAllocations { get; set; }
    public DbSet<ShopPurchaseReturn> ShopPurchaseReturns { get; set; }
    public DbSet<ShopPurchaseReturnItem> ShopPurchaseReturnItems { get; set; }
    public DbSet<ShopCustomer> ShopCustomers { get; set; }
    public DbSet<ShopSale> ShopSales { get; set; }
    public DbSet<ShopSaleItem> ShopSaleItems { get; set; }
    public DbSet<ShopCustomerPayment> ShopCustomerPayments { get; set; }
    public DbSet<ShopCustomerPaymentAllocation> ShopCustomerPaymentAllocations { get; set; }
    public DbSet<ShopSaleReturn> ShopSaleReturns { get; set; }
    public DbSet<ShopSaleReturnItem> ShopSaleReturnItems { get; set; }
    public DbSet<ShopExpenseCategory> ShopExpenseCategories { get; set; }
    public DbSet<ShopExpense> ShopExpenses { get; set; }
    public DbSet<ShopCashRegister> ShopCashRegisters { get; set; }
    public DbSet<ShopCashRegisterTransaction> ShopCashRegisterTransactions { get; set; }
    public DbSet<ShopCashClosing> ShopCashClosings { get; set; }
    public DbSet<ShopBankAccount> ShopBankAccounts { get; set; }
    public DbSet<ShopBankTransaction> ShopBankTransactions { get; set; }
    public DbSet<ShopBankTransfer> ShopBankTransfers { get; set; }
    public DbSet<ShopAiConversation> ShopAiConversations { get; set; }
    public DbSet<ShopAiMessage> ShopAiMessages { get; set; }
    public DbSet<ShopAiActionAudit> ShopAiActionAudits { get; set; }
    public DbSet<ShopAiPendingAction> ShopAiPendingActions { get; set; }
    public DbSet<ShopNotification> ShopNotifications { get; set; }
    public DbSet<ShopNotificationUserState> ShopNotificationUserStates { get; set; }
    public DbSet<ShopNotificationSettings> ShopNotificationSettingsList { get; set; }
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

        builder.Entity<ShopPrintSetting>(b =>
        {
            b.ToTable("ShopPrintSettings", EHubConsts.DbSchema);
            b.ConfigureByConvention();
            b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.HasIndex(x => x.TenantId).IsUnique();
            b.Property(x => x.DefaultPrintPaperSize).HasDefaultValue(ShopPrintPaperSize.Thermal80Mm);
            b.Property(x => x.PrintHeaderLogo).HasDefaultValue(false);
            b.Property(x => x.PrintShopName).HasDefaultValue(true);
            b.Property(x => x.PrintShopAddress).HasDefaultValue(true);
            b.Property(x => x.PrintShopPhone).HasDefaultValue(true);
            b.Property(x => x.PrintShopEmail).HasDefaultValue(false);
            b.Property(x => x.PrintTaxNumber).HasDefaultValue(false);
            b.Property(x => x.PrintFooterMessage).HasMaxLength(ShopPrintSettingConsts.FooterMessageMaxLength);
            b.Property(x => x.PrintTermsAndConditions).HasMaxLength(ShopPrintSettingConsts.TermsAndConditionsMaxLength);
            b.Property(x => x.PrintQrCode).HasDefaultValue(false);
            b.Property(x => x.PrintBarcode).HasDefaultValue(false);
            b.Property(x => x.PrintCustomerCopyLabel).IsRequired().HasMaxLength(ShopPrintSettingConsts.LabelMaxLength).HasDefaultValue("Customer Copy");
            b.Property(x => x.PrintDuplicateCopyLabel).IsRequired().HasMaxLength(ShopPrintSettingConsts.LabelMaxLength).HasDefaultValue("Duplicate Copy");
            b.Property(x => x.PrintItemCode).HasDefaultValue(false);
            b.Property(x => x.PrintUnit).HasDefaultValue(true);
            b.Property(x => x.PrintBatchNumber).HasDefaultValue(true);
            b.Property(x => x.PrintExpiryDate).HasDefaultValue(true);
            b.Property(x => x.PrintDiscount).HasDefaultValue(true);
            b.Property(x => x.PrintTax).HasDefaultValue(true);
            b.Property(x => x.PrintPaymentDetails).HasDefaultValue(true);
            b.Property(x => x.PrintCashierName).HasDefaultValue(true);
            b.Property(x => x.PrintDateTime).HasDefaultValue(true);
            b.Property(x => x.PrintPageNumberForA4).HasDefaultValue(true);
            b.Property(x => x.ThermalFontSize).HasDefaultValue(ShopThermalFontSize.Medium);
            b.Property(x => x.ThermalPrintDensity).HasDefaultValue(ShopThermalPrintDensity.Normal);
            b.Property(x => x.ThermalAutoCut).HasDefaultValue(false);
            b.Property(x => x.ThermalOpenCashDrawer).HasDefaultValue(false);
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
            b.Property(x => x.ExpiryAlertDays);
            b.Property(x => x.BlockExpiredSale).IsRequired().HasDefaultValue(true);
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

        builder.Entity<ShopSale>(b =>
        {
            b.ToTable("ShopSales", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SaleNumber).IsRequired().HasMaxLength(ShopSaleConsts.SaleNumberMaxLength);
            b.Property(x => x.CustomerId).IsRequired();
            b.Property(x => x.SaleDate).IsRequired();
            b.Property(x => x.SaleType).IsRequired().HasConversion<int>().HasDefaultValue(ShopSaleType.Cash);
            b.Property(x => x.PaymentMethod).IsRequired().HasConversion<int>().HasDefaultValue(ShopSalePaymentMethod.Cash);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopSaleStatus.Draft);
            b.Property(x => x.SubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.OtherCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.GrandTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.PaidAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.PendingAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopSaleConsts.ReferenceNumberMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopSaleConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopSaleConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.Sale).HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.CustomerId });
            b.HasIndex(x => new { x.TenantId, x.SaleDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.SaleType });
            b.HasIndex(x => new { x.TenantId, x.Status, x.SaleDate });
            b.HasIndex(x => new { x.TenantId, x.CustomerId, x.SaleDate });
            b.HasIndex(x => new { x.TenantId, x.SaleNumber }).IsUnique();
        });

        builder.Entity<ShopSaleItem>(b =>
        {
            b.ToTable("ShopSaleItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SaleId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopSaleConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopSaleConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopSaleConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopSaleConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.UnitSalePrice).HasPrecision(18, 2);
            b.Property(x => x.UnitCostSnapshot).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineSubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.BatchNumber).HasMaxLength(ShopSaleConsts.BatchNumberMaxLength);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SaleId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.SaleId, x.ProductId });
        });

        builder.Entity<ShopCustomerPayment>(b =>
        {
            b.ToTable("ShopCustomerPayments", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.PaymentNumber).IsRequired().HasMaxLength(ShopCustomerPaymentConsts.PaymentNumberMaxLength);
            b.Property(x => x.CustomerId).IsRequired();
            b.Property(x => x.PaymentDate).IsRequired();
            b.Property(x => x.PaymentType).IsRequired().HasConversion<int>();
            b.Property(x => x.PaymentMethod).IsRequired().HasConversion<int>();
            b.Property(x => x.Amount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopCustomerPaymentConsts.ReferenceNumberMaxLength);
            b.Property(x => x.ChequeNumber).HasMaxLength(ShopCustomerPaymentConsts.ChequeNumberMaxLength);
            b.Property(x => x.BankName).HasMaxLength(ShopCustomerPaymentConsts.BankNameMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopCustomerPaymentConsts.NotesMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopCustomerPaymentStatus.Draft);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopCustomerPaymentConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Allocations).WithOne(x => x.CustomerPayment).HasForeignKey(x => x.CustomerPaymentId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.CustomerId });
            b.HasIndex(x => new { x.TenantId, x.PaymentDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.CustomerId, x.PaymentDate });
            b.HasIndex(x => new { x.TenantId, x.PaymentNumber }).IsUnique();
        });

        builder.Entity<ShopCustomerPaymentAllocation>(b =>
        {
            b.ToTable("ShopCustomerPaymentAllocations", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.CustomerPaymentId).IsRequired();
            b.Property(x => x.SaleId).IsRequired();
            b.Property(x => x.AllocatedAmount).HasPrecision(18, 2).HasDefaultValue(0);

            b.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.CustomerPaymentId });
            b.HasIndex(x => new { x.TenantId, x.SaleId });
            b.HasIndex(x => new { x.CustomerPaymentId, x.SaleId }).IsUnique();
        });

        builder.Entity<ShopSaleReturn>(b =>
        {
            b.ToTable("ShopSaleReturns", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SaleReturnNumber).IsRequired().HasMaxLength(ShopSaleReturnConsts.SaleReturnNumberMaxLength);
            b.Property(x => x.SaleId).IsRequired();
            b.Property(x => x.CustomerId).IsRequired();
            b.Property(x => x.ReturnDate).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopSaleReturnStatus.Draft);
            b.Property(x => x.Reason).IsRequired().HasConversion<int>();
            b.Property(x => x.ReasonDetails).HasMaxLength(ShopSaleReturnConsts.ReasonDetailsMaxLength);
            b.Property(x => x.SettlementType).IsRequired().HasConversion<int>();
            b.Property(x => x.SubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.OtherCharges).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.GrandTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.RefundAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CustomerCreditAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.Notes).HasMaxLength(ShopSaleReturnConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopSaleReturnConsts.CancellationReasonMaxLength);

            b.HasOne<ShopSale>().WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopCustomer>().WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.SaleReturn).HasForeignKey(x => x.SaleReturnId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SaleReturnNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.SaleId });
            b.HasIndex(x => new { x.TenantId, x.CustomerId });
            b.HasIndex(x => new { x.TenantId, x.ReturnDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<ShopSaleReturnItem>(b =>
        {
            b.ToTable("ShopSaleReturnItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SaleReturnId).IsRequired();
            b.Property(x => x.SaleItemId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopSaleReturnConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopSaleReturnConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopSaleReturnConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopSaleReturnConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.BatchNumber).HasMaxLength(ShopSaleReturnConsts.BatchNumberMaxLength);
            b.Property(x => x.SoldQuantitySnapshot).HasPrecision(18, 4);
            b.Property(x => x.PreviouslyReturnedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.ReturnQuantity).HasPrecision(18, 4);
            b.Property(x => x.UnitSalePrice).HasPrecision(18, 2);
            b.Property(x => x.UnitCostSnapshot).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.TaxPercentage).HasPrecision(5, 2).HasDefaultValue(0);
            b.Property(x => x.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineSubTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.LineTotal).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.Reason).IsRequired().HasConversion<int>();
            b.Property(x => x.Notes).HasMaxLength(ShopSaleReturnConsts.NotesMaxLength);

            b.HasOne(x => x.SaleItem).WithMany().HasForeignKey(x => x.SaleItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopProduct>().WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => new { x.TenantId, x.SaleReturnId });
            b.HasIndex(x => new { x.SaleReturnId, x.SaleItemId }).IsUnique();
        });

        builder.Entity<ShopExpenseCategory>(b =>
        {
            b.ToTable("ShopExpenseCategories", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopExpenseCategoryConsts.CodeMaxLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopExpenseCategoryConsts.NameMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopExpenseCategoryConsts.DescriptionMaxLength);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.Name });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<ShopExpense>(b =>
        {
            b.ToTable("ShopExpenses", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.ExpenseNumber).IsRequired().HasMaxLength(ShopExpenseConsts.ExpenseNumberMaxLength);
            b.Property(x => x.ExpenseCategoryId).IsRequired();
            b.Property(x => x.ExpenseDate).IsRequired();
            b.Property(x => x.Amount).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.PaymentMethod).IsRequired().HasConversion<int>().HasDefaultValue(ShopExpensePaymentMethod.Cash);
            b.Property(x => x.PaidTo).HasMaxLength(ShopExpenseConsts.PaidToMaxLength);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopExpenseConsts.ReferenceNumberMaxLength);
            b.Property(x => x.ChequeNumber).HasMaxLength(ShopExpenseConsts.ChequeNumberMaxLength);
            b.Property(x => x.BankName).HasMaxLength(ShopExpenseConsts.BankNameMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopExpenseConsts.DescriptionMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopExpenseConsts.NotesMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopExpenseStatus.Draft);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopExpenseConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.ExpenseCategory).WithMany().HasForeignKey(x => x.ExpenseCategoryId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.ExpenseNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ExpenseCategoryId });
            b.HasIndex(x => new { x.TenantId, x.ExpenseDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Status, x.ExpenseDate });
            b.HasIndex(x => new { x.TenantId, x.ExpenseCategoryId, x.ExpenseDate });
            b.HasIndex(x => new { x.TenantId, x.PaymentMethod });
            b.HasIndex(x => new { x.TenantId, x.PaidTo });
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber });
        });

        builder.Entity<ShopCashRegister>(b =>
        {
            b.ToTable("ShopCashRegisters", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopCashRegisterConsts.CodeMaxLength);
            b.Property(x => x.Name).IsRequired().HasMaxLength(ShopCashRegisterConsts.NameMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopCashRegisterConsts.DescriptionMaxLength);
            b.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.IsDefault });
        });

        builder.Entity<ShopCashClosing>(b =>
        {
            b.ToTable("ShopCashClosings", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.CashRegisterId).IsRequired();
            b.Property(x => x.BusinessDate).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopCashClosingStatus.Open);
            b.Property(x => x.OpeningCash).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CashSales).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CustomerCashPayments).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.SupplierCashPayments).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CashExpenses).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CustomerRefunds).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ManualCashIn).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ManualCashOut).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ExpectedClosingCash).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ActualClosingCash).HasPrecision(18, 2);
            b.Property(x => x.DifferenceAmount).HasPrecision(18, 2);
            b.Property(x => x.Notes).HasMaxLength(ShopCashRegisterConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopCashRegisterConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.CashRegister).WithMany().HasForeignKey(x => x.CashRegisterId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.CashRegisterId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.CashRegisterId, x.BusinessDate });
        });

        builder.Entity<ShopCashRegisterTransaction>(b =>
        {
            b.ToTable("ShopCashRegisterTransactions", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.CashRegisterId).IsRequired();
            b.Property(x => x.TransactionDate).IsRequired();
            b.Property(x => x.TransactionType).IsRequired().HasConversion<int>();
            b.Property(x => x.Direction).IsRequired().HasConversion<int>();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ReferenceType).IsRequired().HasConversion<int>();
            b.Property(x => x.ReferenceId).IsRequired();
            b.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(ShopCashRegisterConsts.ReferenceNumberMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopCashRegisterConsts.NotesMaxLength);

            b.HasOne(x => x.CashRegister).WithMany().HasForeignKey(x => x.CashRegisterId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.CashClosing).WithMany().HasForeignKey(x => x.CashClosingId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.CashRegisterId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId });
            // Includes Direction (beyond the minimal example in the spec) so that an opposite-direction
            // reversal transaction (created when a cash-affecting source is cancelled) can coexist with
            // its original posting under the same reference+type, while still preventing an exact
            // duplicate re-post of the same direction for the same source event.
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId, x.TransactionType, x.Direction }).IsUnique();
        });

        builder.Entity<ShopBankAccount>(b =>
        {
            b.ToTable("ShopBankAccounts", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.Code).IsRequired().HasMaxLength(ShopBankAccountConsts.CodeMaxLength);
            b.Property(x => x.AccountName).IsRequired().HasMaxLength(ShopBankAccountConsts.AccountNameMaxLength);
            b.Property(x => x.BankName).IsRequired().HasMaxLength(ShopBankAccountConsts.BankNameMaxLength);
            b.Property(x => x.AccountNumber).HasMaxLength(ShopBankAccountConsts.AccountNumberMaxLength);
            b.Property(x => x.IBAN).HasMaxLength(ShopBankAccountConsts.IbanMaxLength);
            b.Property(x => x.BranchName).HasMaxLength(ShopBankAccountConsts.BranchNameMaxLength);
            b.Property(x => x.OpeningBalance).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.CurrentBalance).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.IsDefault).IsRequired().HasDefaultValue(false);
            b.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
            b.Property(x => x.Notes).HasMaxLength(ShopBankAccountConsts.NotesMaxLength);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.BankName });
            b.HasIndex(x => new { x.TenantId, x.IsDefault });
            b.HasIndex(x => new { x.TenantId, x.IsActive });
        });

        builder.Entity<ShopBankTransaction>(b =>
        {
            b.ToTable("ShopBankTransactions", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.BankAccountId).IsRequired();
            b.Property(x => x.TransactionDate).IsRequired();
            b.Property(x => x.TransactionType).IsRequired().HasConversion<int>();
            b.Property(x => x.Direction).IsRequired().HasConversion<int>();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ReferenceType).IsRequired().HasConversion<int>();
            b.Property(x => x.ReferenceId).IsRequired();
            b.Property(x => x.ReferenceNumber).IsRequired().HasMaxLength(ShopBankAccountConsts.ReferenceNumberMaxLength);
            b.Property(x => x.Description).HasMaxLength(ShopBankAccountConsts.DescriptionMaxLength);
            b.Property(x => x.BalanceAfterTransaction).HasPrecision(18, 2);
            b.Property(x => x.IsReversal).IsRequired().HasDefaultValue(false);

            b.HasOne(x => x.BankAccount).WithMany().HasForeignKey(x => x.BankAccountId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.BankAccountId });
            b.HasIndex(x => new { x.TenantId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.BankAccountId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.TransactionType });
            b.HasIndex(x => new { x.TenantId, x.Direction });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId });
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber });
            // Guarantees a given source event (an original posting, or its opposite-direction reversal)
            // can only ever create one bank transaction per type (idempotency).
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId, x.TransactionType, x.IsReversal }).IsUnique();
        });

        builder.Entity<ShopBankTransfer>(b =>
        {
            b.ToTable("ShopBankTransfers", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.TransferNumber).IsRequired().HasMaxLength(ShopBankAccountConsts.TransferNumberMaxLength);
            b.Property(x => x.TransferDate).IsRequired();
            b.Property(x => x.TransferType).IsRequired().HasConversion<int>();
            b.Property(x => x.Amount).HasPrecision(18, 2);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopBankAccountConsts.ReferenceNumberMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopBankAccountConsts.NotesMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopBankTransferStatus.Draft);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopBankAccountConsts.CancellationReasonMaxLength);

            b.HasOne(x => x.FromBankAccount).WithMany().HasForeignKey(x => x.FromBankAccountId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ToBankAccount).WithMany().HasForeignKey(x => x.ToBankAccountId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.TransferNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.TransferDate });
            b.HasIndex(x => new { x.TenantId, x.TransferType });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.FromBankAccountId });
            b.HasIndex(x => new { x.TenantId, x.ToBankAccountId });
        });

        builder.Entity<ShopAiConversation>(b =>
        {
            b.ToTable("ShopAiConversations", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Title).HasMaxLength(ShopAiConsts.TitleMaxLength);
            b.Property(x => x.DetectedLanguage).IsRequired().HasConversion<int>().HasDefaultValue(ShopAiLanguage.Unknown);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopAiConversationStatus.Active);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.LastMessageDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
        });

        builder.Entity<ShopAiMessage>(b =>
        {
            b.ToTable("ShopAiMessages", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.ConversationId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Role).IsRequired().HasConversion<int>();
            b.Property(x => x.MessageText).IsRequired().HasMaxLength(ShopAiConsts.MessageTextMaxLength);
            b.Property(x => x.OriginalTranscription).HasMaxLength(ShopAiConsts.TranscriptionMaxLength);
            b.Property(x => x.DetectedLanguage).IsRequired().HasConversion<int>().HasDefaultValue(ShopAiLanguage.Unknown);
            b.Property(x => x.DetectedAction).HasConversion<int?>();
            b.Property(x => x.ActionPayloadJson).HasMaxLength(ShopAiConsts.ActionPayloadJsonMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopAiMessageStatus.Received);
            b.Property(x => x.ErrorCode).HasMaxLength(ShopAiConsts.ErrorCodeMaxLength);
            b.Property(x => x.ErrorMessage).HasMaxLength(ShopAiConsts.ErrorMessageMaxLength);
            b.Property(x => x.ConfirmationTokenHash).HasMaxLength(ShopAiConsts.ConfirmationTokenHashMaxLength);
            b.Property(x => x.ExecutionResultJson).HasMaxLength(ShopAiConsts.ExecutionResultJsonMaxLength);

            b.HasOne<ShopAiConversation>().WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.ConversationId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.UserId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.DetectedAction });
        });

        builder.Entity<ShopAiActionAudit>(b =>
        {
            b.ToTable("ShopAiActionAudits", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.ConversationId).IsRequired();
            b.Property(x => x.MessageId).IsRequired();
            b.Property(x => x.ActionName).IsRequired().HasMaxLength(ShopAiConsts.ActionNameMaxLength);
            b.Property(x => x.SanitizedPayloadJson).IsRequired().HasMaxLength(ShopAiConsts.SanitizedPayloadJsonMaxLength);
            b.Property(x => x.ConfirmationRequired).IsRequired();
            b.Property(x => x.ExecutionStatus).IsRequired().HasConversion<int>();
            b.Property(x => x.ResultReferenceType).HasMaxLength(ShopAiConsts.ResultReferenceTypeMaxLength);
            b.Property(x => x.ErrorCode).HasMaxLength(ShopAiConsts.ErrorCodeMaxLength);
            b.Property(x => x.ErrorMessage).HasMaxLength(ShopAiConsts.ErrorMessageMaxLength);

            b.HasOne<ShopAiConversation>().WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopAiMessage>().WithMany().HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.ActionName, x.CreationTime });
            b.HasIndex(x => new { x.TenantId, x.ExecutionStatus });
        });

        builder.Entity<ShopAiPendingAction>(b =>
        {
            b.ToTable("ShopAiPendingActions", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.ConversationId).IsRequired();
            b.Property(x => x.SourceMessageId).IsRequired();
            b.Property(x => x.ActionType).IsRequired().HasConversion<int>();
            b.Property(x => x.ModuleKey).IsRequired().HasMaxLength(ShopAiConsts.ModuleKeyMaxLength);
            b.Property(x => x.CollectedValuesJson).IsRequired().HasMaxLength(ShopAiConsts.CollectedValuesJsonMaxLength);
            b.Property(x => x.MissingFieldsJson).IsRequired().HasMaxLength(ShopAiConsts.MissingFieldsJsonMaxLength);
            b.Property(x => x.LookupResolutionsJson).IsRequired().HasMaxLength(ShopAiConsts.LookupResolutionsJsonMaxLength);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopAiPendingActionStatus.CollectingInformation);
            b.Property(x => x.ExpiryDate).IsRequired();
            b.Property(x => x.ConfirmationTokenHash).HasMaxLength(ShopAiConsts.ConfirmationTokenHashMaxLength);

            b.HasOne<ShopAiConversation>().WithMany().HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopAiMessage>().WithMany().HasForeignKey(x => x.SourceMessageId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.ConversationId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.ExpiryDate });
        });

        builder.Entity<ShopNotification>(b =>
        {
            b.ToTable("ShopNotifications", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.UserId);
            b.Property(x => x.Type).IsRequired().HasConversion<int>();
            b.Property(x => x.Severity).IsRequired().HasConversion<int>();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopNotificationStatus.Unread);
            b.Property(x => x.Title).IsRequired().HasMaxLength(ShopNotificationConsts.TitleMaxLength);
            b.Property(x => x.Message).IsRequired().HasMaxLength(ShopNotificationConsts.MessageMaxLength);
            b.Property(x => x.ReferenceType).HasMaxLength(ShopNotificationConsts.ReferenceTypeMaxLength);
            b.Property(x => x.ReferenceNumber).HasMaxLength(ShopNotificationConsts.ReferenceNumberMaxLength);
            b.Property(x => x.NavigationUrl).HasMaxLength(ShopNotificationConsts.NavigationUrlMaxLength);
            b.Property(x => x.ActionLabel).HasMaxLength(ShopNotificationConsts.ActionLabelMaxLength);
            b.Property(x => x.SourceKey).IsRequired().HasMaxLength(ShopNotificationConsts.SourceKeyMaxLength);
            b.Property(x => x.SourceDataJson).HasMaxLength(ShopNotificationConsts.SourceDataJsonMaxLength);
            b.Property(x => x.TriggeredDate).IsRequired();

            b.HasIndex(x => x.TenantId);
            // At most one *active* (non-Resolved) notification per condition; unlimited Resolved history rows may share the same SourceKey.
            b.HasIndex(x => new { x.TenantId, x.SourceKey }).IsUnique().HasFilter("[Status] <> 3");
            b.HasIndex(x => new { x.TenantId, x.UserId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Type, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Severity, x.Status });
            b.HasIndex(x => new { x.TenantId, x.TriggeredDate });
            b.HasIndex(x => new { x.TenantId, x.ExpiresDate });
        });

        builder.Entity<ShopNotificationUserState>(b =>
        {
            b.ToTable("ShopNotificationUserStates", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.NotificationId).IsRequired();
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopNotificationStatus.Unread);

            b.HasOne<ShopNotification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.UserId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.NotificationId, x.UserId }).IsUnique();
        });

        builder.Entity<ShopNotificationSettings>(b =>
        {
            b.ToTable("ShopNotificationSettingsList", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.BankLowBalanceThreshold).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.ProfitLossWarningThreshold).HasPrecision(5, 2).HasDefaultValue(0);

            b.HasIndex(x => x.TenantId).IsUnique();
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
            b.HasIndex(x => new { x.TenantId, x.Status, x.ReceiptDate });
            b.HasIndex(x => new { x.TenantId, x.SupplierId, x.ReceiptDate });
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
            b.Property(x => x.BatchBalanceQuantity).HasPrecision(18, 4);
            b.Property(x => x.Notes).HasMaxLength(ShopStockTransactionConsts.NotesMaxLength);
            b.Property(x => x.CreationTime).IsRequired();

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopProductBatch>().WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.TransactionType });
            b.HasIndex(x => new { x.TenantId, x.ProductId, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.TransactionType, x.TransactionDate });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.ReferenceId });
            b.HasIndex(x => new { x.TenantId, x.ReferenceNumber });
            b.HasIndex(x => new { x.TenantId, x.ProductBatchId });
            b.HasIndex(x => new { x.TenantId, x.ReferenceType, x.SourceItemId }).IsUnique();
        });

        builder.Entity<ShopStockAdjustment>(b =>
        {
            b.ToTable("ShopStockAdjustments", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.AdjustmentNumber).IsRequired().HasMaxLength(ShopStockAdjustmentConsts.AdjustmentNumberMaxLength);
            b.Property(x => x.AdjustmentDate).IsRequired();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopStockAdjustmentStatus.Draft);
            b.Property(x => x.Reason).IsRequired().HasConversion<int>();
            b.Property(x => x.ReasonDetails).HasMaxLength(ShopStockAdjustmentConsts.ReasonDetailsMaxLength);
            b.Property(x => x.Notes).HasMaxLength(ShopStockAdjustmentConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopStockAdjustmentConsts.CancellationReasonMaxLength);

            b.HasMany(x => x.Items).WithOne(x => x.StockAdjustment).HasForeignKey(x => x.StockAdjustmentId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.AdjustmentNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.AdjustmentDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Reason });
        });

        builder.Entity<ShopStockAdjustmentItem>(b =>
        {
            b.ToTable("ShopStockAdjustmentItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.StockAdjustmentId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopStockAdjustmentConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopStockAdjustmentConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopStockAdjustmentConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopStockAdjustmentConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.AdjustmentType).IsRequired().HasConversion<int>();
            b.Property(x => x.SystemQuantitySnapshot).HasPrecision(18, 4);
            b.Property(x => x.AdjustmentQuantity).HasPrecision(18, 4);
            b.Property(x => x.FinalQuantity).HasPrecision(18, 4);
            b.Property(x => x.UnitCostSnapshot).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.BatchNumber).HasMaxLength(ShopStockAdjustmentConsts.BatchNumberMaxLength);
            b.Property(x => x.Reason).IsRequired().HasConversion<int>();
            b.Property(x => x.Notes).HasMaxLength(ShopStockAdjustmentConsts.ItemNotesMaxLength);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopProductBatch>().WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.StockAdjustmentId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            // A product may appear more than once per adjustment when different batches are involved,
            // so uniqueness is on (adjustment, product, batch) rather than (adjustment, product) alone.
            b.HasIndex(x => new { x.StockAdjustmentId, x.ProductId, x.ProductBatchId }).IsUnique();
        });

        builder.Entity<ShopStockCount>(b =>
        {
            b.ToTable("ShopStockCounts", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.StockCountNumber).IsRequired().HasMaxLength(ShopStockCountConsts.StockCountNumberMaxLength);
            b.Property(x => x.CountDate).IsRequired();
            b.Property(x => x.Scope).IsRequired().HasConversion<int>();
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopStockCountStatus.Draft);
            b.Property(x => x.Notes).HasMaxLength(ShopStockCountConsts.NotesMaxLength);
            b.Property(x => x.CancellationReason).HasMaxLength(ShopStockCountConsts.CancellationReasonMaxLength);

            b.HasOne<ShopProductCategory>().WithMany().HasForeignKey(x => x.ProductCategoryId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopStockAdjustment>().WithMany().HasForeignKey(x => x.GeneratedStockAdjustmentId).OnDelete(DeleteBehavior.Restrict);
            b.HasMany(x => x.Items).WithOne(x => x.StockCount).HasForeignKey(x => x.StockCountId).OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.StockCountNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.CountDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.Scope });
            b.HasIndex(x => new { x.TenantId, x.ProductCategoryId });
            b.HasIndex(x => new { x.TenantId, x.GeneratedStockAdjustmentId });
        });

        builder.Entity<ShopStockCountItem>(b =>
        {
            b.ToTable("ShopStockCountItems", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.StockCountId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductCodeSnapshot).IsRequired().HasMaxLength(ShopStockCountConsts.ProductCodeSnapshotMaxLength);
            b.Property(x => x.ProductNameSnapshot).IsRequired().HasMaxLength(ShopStockCountConsts.ProductNameSnapshotMaxLength);
            b.Property(x => x.UnitNameSnapshot).IsRequired().HasMaxLength(ShopStockCountConsts.UnitNameSnapshotMaxLength);
            b.Property(x => x.UnitShortNameSnapshot).IsRequired().HasMaxLength(ShopStockCountConsts.UnitShortNameSnapshotMaxLength);
            b.Property(x => x.BatchNumberSnapshot).HasMaxLength(ShopProductBatchConsts.BatchNumberMaxLength);
            b.Property(x => x.SystemQuantitySnapshot).HasPrecision(18, 4);
            b.Property(x => x.PhysicalQuantity).HasPrecision(18, 4);
            b.Property(x => x.DifferenceQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.AdjustmentType).HasConversion<int>();
            b.Property(x => x.IsCounted).IsRequired().HasDefaultValue(false);
            b.Property(x => x.Notes).HasMaxLength(ShopStockCountConsts.ItemNotesMaxLength);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.StockCountId });
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            // Batch-tracked products contribute one row per batch, so uniqueness includes the batch.
            b.HasIndex(x => new { x.StockCountId, x.ProductId, x.ProductBatchId }).IsUnique();
        });

        builder.Entity<ShopProductBatch>(b =>
        {
            b.ToTable("ShopProductBatches", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.BatchNumber).IsRequired().HasMaxLength(ShopProductBatchConsts.BatchNumberMaxLength);
            b.Property(x => x.NormalizedBatchNumber).IsRequired().HasMaxLength(ShopProductBatchConsts.NormalizedBatchNumberMaxLength);
            b.Property(x => x.ReceivedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.IssuedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.AvailableQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.ReservedQuantity).HasPrecision(18, 4).HasDefaultValue(0);
            b.Property(x => x.UnitCost).HasPrecision(18, 2).HasDefaultValue(0);
            b.Property(x => x.Status).IsRequired().HasConversion<int>().HasDefaultValue(ShopProductBatchStatus.Active);
            b.Property(x => x.Notes).HasMaxLength(ShopProductBatchConsts.NotesMaxLength);
            b.Property(x => x.IsBlocked).IsRequired().HasDefaultValue(false);
            b.Property(x => x.BlockReason).HasMaxLength(ShopProductBatchConsts.BlockReasonMaxLength);

            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopSupplier>().WithMany().HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopGoodsReceipt>().WithMany().HasForeignKey(x => x.GoodsReceiptId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne<ShopGoodsReceiptItem>().WithMany().HasForeignKey(x => x.GoodsReceiptItemId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.ProductId });
            b.HasIndex(x => new { x.TenantId, x.ProductId, x.NormalizedBatchNumber }).IsUnique();
            b.HasIndex(x => new { x.TenantId, x.ExpiryDate });
            b.HasIndex(x => new { x.TenantId, x.Status });
            b.HasIndex(x => new { x.TenantId, x.ExpiryDate, x.Status });
            b.HasIndex(x => new { x.TenantId, x.SupplierId });
            b.HasIndex(x => new { x.TenantId, x.AvailableQuantity });
        });

        builder.Entity<ShopSaleItemBatchAllocation>(b =>
        {
            b.ToTable("ShopSaleItemBatchAllocations", EHubConsts.DbSchema); b.ConfigureByConvention(); b.HasKey(x => x.Id);
            b.Property(x => x.TenantId).IsRequired();
            b.Property(x => x.SaleId).IsRequired();
            b.Property(x => x.SaleItemId).IsRequired();
            b.Property(x => x.ProductId).IsRequired();
            b.Property(x => x.ProductBatchId).IsRequired();
            b.Property(x => x.BatchNumberSnapshot).IsRequired().HasMaxLength(ShopProductBatchConsts.BatchNumberMaxLength);
            b.Property(x => x.Quantity).HasPrecision(18, 4);
            b.Property(x => x.UnitCostSnapshot).HasPrecision(18, 2).HasDefaultValue(0);

            b.HasOne(x => x.Sale).WithMany().HasForeignKey(x => x.SaleId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.SaleItem).WithMany().HasForeignKey(x => x.SaleItemId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
            b.HasOne(x => x.ProductBatch).WithMany().HasForeignKey(x => x.ProductBatchId).OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.TenantId);
            b.HasIndex(x => new { x.TenantId, x.SaleId });
            b.HasIndex(x => new { x.TenantId, x.SaleItemId });
            b.HasIndex(x => new { x.TenantId, x.ProductBatchId });
            b.HasIndex(x => new { x.SaleItemId, x.ProductBatchId }).IsUnique();
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
            b.HasIndex(x => new { x.TenantId, x.SupplierId, x.PaymentDate });
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
