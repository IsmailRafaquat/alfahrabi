using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AbpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ImpersonatorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImpersonatorUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ImpersonatorTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ImpersonatorTenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    HttpMethod = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    Url = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Exceptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpBackgroundJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    JobName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    JobArgs = table.Column<string>(type: "nvarchar(max)", maxLength: 1048576, nullable: false),
                    TryCount = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextTryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAbandoned = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false, defaultValue: (byte)15),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBackgroundJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpBlobContainers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBlobContainers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpClaimTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    Regex = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    RegexDescription = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpClaimTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableToHost = table.Column<bool>(type: "bit", nullable: false),
                    AllowedProviders = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ValueType = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpFeatureValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpFeatureValues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpLinkUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpLinkUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpOrganizationUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(95)", maxLength: 95, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpOrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnits_AbpOrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissionGrants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGrants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissionGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissionGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ParentName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MultiTenancySide = table.Column<byte>(type: "tinyint", nullable: false),
                    Providers = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StateCheckers = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpPermissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    IsStatic = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSecurityLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApplicationName = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    Identity = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    Action = table.Column<string>(type: "nvarchar(96)", maxLength: 96, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    TenantName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CorrelationId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ClientIpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSecurityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Device = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DeviceInfo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IpAddresses = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    SignedIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastAccessed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettingDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsVisibleToClients = table.Column<bool>(type: "bit", nullable: false),
                    Providers = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    IsInherited = table.Column<bool>(type: "bit", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "bit", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettingDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    NormalizedName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserDelegations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserDelegations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Surname = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsExternal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ShouldChangePasswordOnNextLogin = table.Column<bool>(type: "bit", nullable: false),
                    EntityVersion = table.Column<int>(type: "int", nullable: false),
                    LastPasswordChangeTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EntryLimitType = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFeeHeads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ChargeType = table.Column<int>(type: "int", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFeeHeads", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppFeeStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeLevel = table.Column<int>(type: "int", nullable: false),
                    Shift = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFeeStructures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppLateFeePolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeLevel = table.Column<int>(type: "int", nullable: true),
                    Section = table.Column<int>(type: "int", nullable: true),
                    Shift = table.Column<int>(type: "int", nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    GraceDays = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppLateFeePolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStaffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PhoneNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nationality = table.Column<int>(type: "int", nullable: false),
                    RelationshipStatus = table.Column<int>(type: "int", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    LanguageKnown = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    DisabilityStatus = table.Column<int>(type: "int", nullable: false),
                    StreetAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StreetAddressLine2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    City = table.Column<int>(type: "int", nullable: false),
                    Province = table.Column<int>(type: "int", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Department = table.Column<int>(type: "int", nullable: false),
                    EmploymentType = table.Column<int>(type: "int", nullable: false),
                    JobStatus = table.Column<int>(type: "int", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReportingManager = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    WorkShift = table.Column<int>(type: "int", nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStaffs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppStudents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdmissionNo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    RollNo = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    StreetAddressLine2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    City = table.Column<int>(type: "int", nullable: false),
                    Province = table.Column<int>(type: "int", nullable: false),
                    ZipCode = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    PFirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PLastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PRelatonShipToStudent = table.Column<int>(type: "int", nullable: false),
                    PPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ECFirstName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ECLastName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ECRelationShipToStudent = table.Column<int>(type: "int", nullable: true),
                    ECPhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    ECEmail = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    GradeLevel = table.Column<int>(type: "int", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    Shift = table.Column<int>(type: "int", nullable: false),
                    PerviousSchool = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    StudentIdNo = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Extracurrucular = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Commnets = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Accommodations = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    GradeLevel = table.Column<int>(type: "int", nullable: true),
                    CreditHours = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppTeacherSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectIds = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: true),
                    WeeklyHours = table.Column<int>(type: "int", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsPrimaryTeacher = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppTeacherSubjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JsonWebKeySet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostLogoutRedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Settings = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUri = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resources = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictScopes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopAiConversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DetectedLanguage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastMessageDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiConversations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopBankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CurrentBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopBankAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopCashRegisters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCashRegisters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopCustomers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ContactPerson = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateOrProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsWalkInCustomer = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCustomers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopDocumentSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LastNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopDocumentSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReferenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    NavigationUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ActionLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    SourceKey = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SourceDataJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TriggeredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopNotificationSettingsList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LowStockNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    OutOfStockNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NearExpiryNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ExpiredBatchNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CustomerDueNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SupplierDueNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CashDifferenceNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    BankLowBalanceNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PendingDraftNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    ProfitLossWarningNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NearExpiryDefaultDays = table.Column<int>(type: "int", nullable: false),
                    CustomerDueReminderDays = table.Column<int>(type: "int", nullable: false),
                    SupplierDueReminderDays = table.Column<int>(type: "int", nullable: false),
                    DraftPendingHours = table.Column<int>(type: "int", nullable: false),
                    BankLowBalanceThreshold = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ProfitLossWarningThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    NotificationRetentionDays = table.Column<int>(type: "int", nullable: false),
                    EmailNotificationsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LastExpiryCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastProfitLossCheckDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotificationSettingsList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopPrintSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DefaultPrintPaperSize = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    PrintHeaderLogo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintShopName = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintShopAddress = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintShopPhone = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintShopEmail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintTaxNumber = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintFooterMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrintTermsAndConditions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PrintQrCode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintBarcode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintCustomerCopyLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Customer Copy"),
                    PrintDuplicateCopyLabel = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, defaultValue: "Duplicate Copy"),
                    PrintItemCode = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    PrintUnit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintBatchNumber = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintExpiryDate = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintDiscount = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintTax = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintPaymentDetails = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintCashierName = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintDateTime = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    PrintPageNumberForA4 = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ThermalFontSize = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ThermalPrintDensity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    ThermalAutoCut = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ThermalOpenCashDrawer = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPrintSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopProductCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopProductCategories_ShopProductCategories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "ShopProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopDisplayName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    LogoFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    StateOrProvince = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "PKR"),
                    CurrencySymbol = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false, defaultValue: "₨"),
                    TaxNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DefaultTaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    InvoicePrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "INV"),
                    PurchaseOrderPrefix = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PO"),
                    ReceiptFooter = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReturnPolicy = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AllowNegativeStock = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AutoGenerateProductBarcode = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AutoGenerateInvoiceQrCode = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    DefaultLowStockLevel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 5m),
                    DecimalPlaces = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    IsConfigured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustmentNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AdjustmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopSuppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    AlternatePhone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine1 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    AddressLine2 = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StateOrProvince = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PostalCode = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSuppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShopUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ShortName = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AllowDecimal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopUnits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AbpAuditLogActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ServiceName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    MethodName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Parameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ExecutionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutionDuration = table.Column<int>(type: "int", nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpAuditLogActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpAuditLogActions_AbpAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AbpAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpEntityChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangeTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangeType = table.Column<byte>(type: "tinyint", nullable: false),
                    EntityTenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    EntityTypeFullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpEntityChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpEntityChanges_AbpAuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AbpAuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpBlobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContainerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", maxLength: 2147483647, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpBlobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpBlobs_AbpBlobContainers_ContainerId",
                        column: x => x.ContainerId,
                        principalTable: "AbpBlobContainers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpOrganizationUnitRoles",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpOrganizationUnitRoles", x => new { x.OrganizationUnitId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnitRoles_AbpOrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpOrganizationUnitRoles_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpRoleClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpRoleClaims_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpTenantConnectionStrings",
                columns: table => new
                {
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpTenantConnectionStrings", x => new { x.TenantId, x.Name });
                    table.ForeignKey(
                        name: "FK_AbpTenantConnectionStrings_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClaimType = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ClaimValue = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpUserClaims_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserLogins",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProviderKey = table.Column<string>(type: "nvarchar(196)", maxLength: 196, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserLogins", x => new { x.UserId, x.LoginProvider });
                    table.ForeignKey(
                        name: "FK_AbpUserLogins_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserOrganizationUnits",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserOrganizationUnits", x => new { x.OrganizationUnitId, x.UserId });
                    table.ForeignKey(
                        name: "FK_AbpUserOrganizationUnits_AbpOrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "AbpOrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpUserOrganizationUnits_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AbpUserRoles_AbpRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AbpRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AbpUserRoles_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbpUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AbpUserTokens_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppExpenseEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpenseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidTo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppExpenseEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppExpenseEntries_AppExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "AppExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppFeeStructureItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FeeStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeHeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppFeeStructureItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppFeeStructureItems_AppFeeHeads_FeeHeadId",
                        column: x => x.FeeHeadId,
                        principalTable: "AppFeeHeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppFeeStructureItems_AppFeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "AppFeeStructures",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppStaffAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStaffAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStaffAttendances_AppStaffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AppStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStaffDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffDT = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    FileAttachments_Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FileAttachments_Path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    FileAttachments_FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileAttachments_BlobName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStaffDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStaffDocuments_AppStaffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AppStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStaffSalaryPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalaryMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalaryAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStaffSalaryPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStaffSalaryPayments_AppStaffs_StaffId",
                        column: x => x.StaffId,
                        principalTable: "AppStaffs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppStudentAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentAttendances_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStudentDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    FileAttachment_Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FileAttachment_Path = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    FileAttachment_FileExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileAttachment_BlobName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentDocuments_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStudentFeeDiscounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeHeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DiscountType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: true),
                    EndMonth = table.Column<int>(type: "int", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ApprovedByStaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentFeeDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentFeeDiscounts_AppFeeHeads_FeeHeadId",
                        column: x => x.FeeHeadId,
                        principalTable: "AppFeeHeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStudentFeeDiscounts_AppStaffs_ApprovedByStaffId",
                        column: x => x.ApprovedByStaffId,
                        principalTable: "AppStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStudentFeeDiscounts_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStudentFeeProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeStructureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentFeeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId",
                        column: x => x.FeeStructureId,
                        principalTable: "AppFeeStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppStudentFeeProfiles_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppStudentMonthlyFees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentMonthlyFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentMonthlyFees_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictAuthorizations_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShopAiMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    OriginalTranscription = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    DetectedLanguage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DetectedAction = table.Column<int>(type: "int", nullable: true),
                    ActionPayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConfirmationTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ConfirmationExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionResultJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiMessages_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopBankTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BalanceAfterTransaction = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReversalOfTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsReversal = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopBankTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopBankTransactions_ShopBankAccounts_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopBankTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransferNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransferType = table.Column<int>(type: "int", nullable: false),
                    FromBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToBankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopBankTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopBankTransfers_ShopBankAccounts_FromBankAccountId",
                        column: x => x.FromBankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopBankTransfers_ShopBankAccounts_ToBankAccountId",
                        column: x => x.ToBankAccountId,
                        principalTable: "ShopBankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopCashClosings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BusinessDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    OpeningCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CashSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerCashPayments = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    SupplierCashPayments = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CashExpenses = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerRefunds = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ManualCashIn = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ManualCashOut = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ExpectedClosingCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ActualClosingCash = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DifferenceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OpenedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpenedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCashClosings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCashClosings_ShopCashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "ShopCashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopCustomerPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ChequeNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCustomerPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCustomerPayments_ShopCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "ShopCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SaleType = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PendingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSales_ShopCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "ShopCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopExpenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ExpenseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PaidTo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ChequeNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopExpenses_ShopExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ShopExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopNotificationUserStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DismissedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopNotificationUserStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopNotificationUserStates_ShopNotifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "ShopNotifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockCountNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CountDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Scope = table.Column<int>(type: "int", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GeneratedStockAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CountedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockCounts_ShopProductCategories_ProductCategoryId",
                        column: x => x.ProductCategoryId,
                        principalTable: "ShopProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockCounts_ShopStockAdjustments_GeneratedStockAdjustmentId",
                        column: x => x.GeneratedStockAdjustmentId,
                        principalTable: "ShopStockAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopPurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SupplierReference = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ShippingCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RejectedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseOrders_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSupplierPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ChequeNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PostedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSupplierPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSupplierPayments_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Barcode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    SalePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    WholesalePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    MinimumSalePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    CurrentStock = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    MinimumStockLevel = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    MaximumStockLevel = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ReorderLevel = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    TrackBatch = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TrackExpiry = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ExpiryAlertDays = table.Column<int>(type: "int", nullable: true),
                    BlockExpiredSale = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TrackSerialNumber = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopProducts_ShopProductCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "ShopProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProducts_ShopUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "ShopUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AbpEntityPropertyChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EntityChangeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OriginalValue = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    PropertyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PropertyTypeFullName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbpEntityPropertyChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbpEntityPropertyChanges_AbpEntityChanges_EntityChangeId",
                        column: x => x.EntityChangeId,
                        principalTable: "AbpEntityChanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppStudentMonthlyFeeLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StudentMonthlyFeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeeHeadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpectedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdjustmentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LateFeeAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OutstandingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppStudentMonthlyFeeLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppStudentMonthlyFeeLines_AppFeeHeads_FeeHeadId",
                        column: x => x.FeeHeadId,
                        principalTable: "AppFeeHeads",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppStudentMonthlyFeeLines_AppStudentMonthlyFees_StudentMonthlyFeeId",
                        column: x => x.StudentMonthlyFeeId,
                        principalTable: "AppStudentMonthlyFees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ShopAiActionAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SanitizedPayloadJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    ConfirmationRequired = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExecutionStatus = table.Column<int>(type: "int", nullable: false),
                    ResultReferenceType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ResultReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ErrorCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiActionAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiActionAudits_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAiActionAudits_ShopAiMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ShopAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopAiPendingActions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ModuleKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CollectedValuesJson = table.Column<string>(type: "nvarchar(max)", maxLength: 8000, nullable: false),
                    MissingFieldsJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    LookupResolutionsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmationTokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopAiPendingActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopAiPendingActions_ShopAiConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ShopAiConversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopAiPendingActions_ShopAiMessages_SourceMessageId",
                        column: x => x.SourceMessageId,
                        principalTable: "ShopAiMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopCashRegisterTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashRegisterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashClosingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCashRegisterTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCashRegisterTransactions_ShopCashClosings_CashClosingId",
                        column: x => x.CashClosingId,
                        principalTable: "ShopCashClosings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopCashRegisterTransactions_ShopCashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "ShopCashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopCustomerPaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopCustomerPaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopCustomerPaymentAllocations_ShopCustomerPayments_CustomerPaymentId",
                        column: x => x.CustomerPaymentId,
                        principalTable: "ShopCustomerPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopCustomerPaymentAllocations_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleReturnNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SettlementType = table.Column<int>(type: "int", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CustomerCreditAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturns_ShopCustomers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "ShopCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturns_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopGoodsReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierInvoiceNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ReceiptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ShippingCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReceivedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopGoodsReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopGoodsReceipts_ShopPurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "ShopPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopGoodsReceipts_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopPurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    UnitPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseOrderItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseOrderItems_ShopPurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "ShopPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItems_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopPurchaseReturns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseReturnNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    ReasonDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OtherCharges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GrandTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPurchaseReturns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseReturns_ShopGoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "ShopGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseReturns_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSupplierPaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSupplierPaymentAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSupplierPaymentAllocations_ShopGoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "ShopGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSupplierPaymentAllocations_ShopSupplierPayments_SupplierPaymentId",
                        column: x => x.SupplierPaymentId,
                        principalTable: "ShopSupplierPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopGoodsReceiptItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseOrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    OrderedQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviouslyReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    BonusQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    PurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopGoodsReceiptItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopGoodsReceiptItems_ShopGoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "ShopGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShopGoodsReceiptItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopGoodsReceiptItems_ShopPurchaseOrderItems_PurchaseOrderItemId",
                        column: x => x.PurchaseOrderItemId,
                        principalTable: "ShopPurchaseOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoldQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviouslyReturnedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitSalePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopSaleItems_SaleItemId",
                        column: x => x.SaleItemId,
                        principalTable: "ShopSaleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleReturnItems_ShopSaleReturns_SaleReturnId",
                        column: x => x.SaleReturnId,
                        principalTable: "ShopSaleReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopProductBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    NormalizedBatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    IssuedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    ReservedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    FirstReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastMovementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoodsReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GoodsReceiptItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    BlockReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopProductBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopGoodsReceiptItems_GoodsReceiptItemId",
                        column: x => x.GoodsReceiptItemId,
                        principalTable: "ShopGoodsReceiptItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopGoodsReceipts_GoodsReceiptId",
                        column: x => x.GoodsReceiptId,
                        principalTable: "ShopGoodsReceipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopProductBatches_ShopSuppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "ShopSuppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopPurchaseReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodsReceiptItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BatchNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PreviouslyReturnedQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReturnQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPurchasePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineSubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopPurchaseReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseReturnItems_ShopGoodsReceiptItems_GoodsReceiptItemId",
                        column: x => x.GoodsReceiptItemId,
                        principalTable: "ShopGoodsReceiptItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseReturnItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopPurchaseReturnItems_ShopPurchaseReturns_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "ShopPurchaseReturns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopSaleItemBatchAllocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SaleItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ExpiryDateSnapshot = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopSaleItemBatchAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ShopProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopSaleItems_SaleItemId",
                        column: x => x.SaleItemId,
                        principalTable: "ShopSaleItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopSaleItemBatchAllocations_ShopSales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "ShopSales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockAdjustmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    AdjustmentType = table.Column<int>(type: "int", nullable: false),
                    SystemQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    AdjustmentQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FinalQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitCostSnapshot = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ManufacturingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockAdjustmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockAdjustmentItems_ShopProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ShopProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockAdjustmentItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockAdjustmentItems_ShopStockAdjustments_StockAdjustmentId",
                        column: x => x.StockAdjustmentId,
                        principalTable: "ShopStockAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockCountItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockCountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductCodeSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ProductNameSnapshot = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitNameSnapshot = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    UnitShortNameSnapshot = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ProductBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchNumberSnapshot = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDateSnapshot = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SystemQuantitySnapshot = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    PhysicalQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DifferenceQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    AdjustmentType = table.Column<int>(type: "int", nullable: true),
                    IsCounted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CountedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockCountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockCountItems_ShopProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ShopProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockCountItems_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockCountItems_ShopStockCounts_StockCountId",
                        column: x => x.StockCountId,
                        principalTable: "ShopStockCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShopStockTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SourceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuantityIn = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    QuantityOut = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    BalanceQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false, defaultValue: 0m),
                    UnitCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BatchNumber = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchBalanceQuantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopStockTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShopStockTransactions_ShopProductBatches_ProductBatchId",
                        column: x => x.ProductBatchId,
                        principalTable: "ShopProductBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShopStockTransactions_ShopProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ShopProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_AuditLogId",
                table: "AbpAuditLogActions",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogActions_TenantId_ServiceName_MethodName_ExecutionTime",
                table: "AbpAuditLogActions",
                columns: new[] { "TenantId", "ServiceName", "MethodName", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_TenantId_ExecutionTime",
                table: "AbpAuditLogs",
                columns: new[] { "TenantId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpAuditLogs_TenantId_UserId_ExecutionTime",
                table: "AbpAuditLogs",
                columns: new[] { "TenantId", "UserId", "ExecutionTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpBackgroundJobs_IsAbandoned_NextTryTime",
                table: "AbpBackgroundJobs",
                columns: new[] { "IsAbandoned", "NextTryTime" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpBlobContainers_TenantId_Name",
                table: "AbpBlobContainers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpBlobs_ContainerId",
                table: "AbpBlobs",
                column: "ContainerId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpBlobs_TenantId_ContainerId_Name",
                table: "AbpBlobs",
                columns: new[] { "TenantId", "ContainerId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityChanges_AuditLogId",
                table: "AbpEntityChanges",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityChanges_TenantId_EntityTypeFullName_EntityId",
                table: "AbpEntityChanges",
                columns: new[] { "TenantId", "EntityTypeFullName", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpEntityPropertyChanges_EntityChangeId",
                table: "AbpEntityPropertyChanges",
                column: "EntityChangeId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatureGroups_Name",
                table: "AbpFeatureGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatures_GroupName",
                table: "AbpFeatures",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatures_Name",
                table: "AbpFeatures",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpFeatureValues_Name_ProviderName_ProviderKey",
                table: "AbpFeatureValues",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[ProviderName] IS NOT NULL AND [ProviderKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbpLinkUsers_SourceUserId_SourceTenantId_TargetUserId_TargetTenantId",
                table: "AbpLinkUsers",
                columns: new[] { "SourceUserId", "SourceTenantId", "TargetUserId", "TargetTenantId" },
                unique: true,
                filter: "[SourceTenantId] IS NOT NULL AND [TargetTenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnitRoles_RoleId_OrganizationUnitId",
                table: "AbpOrganizationUnitRoles",
                columns: new[] { "RoleId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_Code",
                table: "AbpOrganizationUnits",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_AbpOrganizationUnits_ParentId",
                table: "AbpOrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissionGrants_TenantId_Name_ProviderName_ProviderKey",
                table: "AbpPermissionGrants",
                columns: new[] { "TenantId", "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissionGroups_Name",
                table: "AbpPermissionGroups",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissions_GroupName",
                table: "AbpPermissions",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpPermissions_Name",
                table: "AbpPermissions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoleClaims_RoleId",
                table: "AbpRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpRoles_NormalizedName",
                table: "AbpRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_Action",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_ApplicationName",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "ApplicationName" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_Identity",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "Identity" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSecurityLogs_TenantId_UserId",
                table: "AbpSecurityLogs",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_Device",
                table: "AbpSessions",
                column: "Device");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_SessionId",
                table: "AbpSessions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpSessions_TenantId_UserId",
                table: "AbpSessions",
                columns: new[] { "TenantId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettingDefinitions_Name",
                table: "AbpSettingDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpSettings_Name_ProviderName_ProviderKey",
                table: "AbpSettings",
                columns: new[] { "Name", "ProviderName", "ProviderKey" },
                unique: true,
                filter: "[ProviderName] IS NOT NULL AND [ProviderKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_Name",
                table: "AbpTenants",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AbpTenants_NormalizedName",
                table: "AbpTenants",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserClaims_UserId",
                table: "AbpUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserLogins_LoginProvider_ProviderKey",
                table: "AbpUserLogins",
                columns: new[] { "LoginProvider", "ProviderKey" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserOrganizationUnits_UserId_OrganizationUnitId",
                table: "AbpUserOrganizationUnits",
                columns: new[] { "UserId", "OrganizationUnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUserRoles_RoleId_UserId",
                table: "AbpUserRoles",
                columns: new[] { "RoleId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_Email",
                table: "AbpUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_NormalizedEmail",
                table: "AbpUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_NormalizedUserName",
                table: "AbpUsers",
                column: "NormalizedUserName");

            migrationBuilder.CreateIndex(
                name: "IX_AbpUsers_UserName",
                table: "AbpUsers",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_AppExpenseEntries_ExpenseCategoryId",
                table: "AppExpenseEntries",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFeeStructureItems_FeeHeadId",
                table: "AppFeeStructureItems",
                column: "FeeHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AppFeeStructureItems_FeeStructureId",
                table: "AppFeeStructureItems",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffAttendances_StaffId",
                table: "AppStaffAttendances",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffDocuments_IsVerified",
                table: "AppStaffDocuments",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffDocuments_StaffDT",
                table: "AppStaffDocuments",
                column: "StaffDT");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffDocuments_StaffId",
                table: "AppStaffDocuments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffs_Department",
                table: "AppStaffs",
                column: "Department");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffs_Email",
                table: "AppStaffs",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffs_JobStatus",
                table: "AppStaffs",
                column: "JobStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffs_PhoneNo",
                table: "AppStaffs",
                column: "PhoneNo");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffs_TenantId_EmployeeCode",
                table: "AppStaffs",
                columns: new[] { "TenantId", "EmployeeCode" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppStaffSalaryPayments_StaffId",
                table: "AppStaffSalaryPayments",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentAttendances_StudentId",
                table: "AppStudentAttendances",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentDocuments_DocumentType",
                table: "AppStudentDocuments",
                column: "DocumentType");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentDocuments_IsVerified",
                table: "AppStudentDocuments",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentDocuments_StudentId",
                table: "AppStudentDocuments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeDiscounts_ApprovedByStaffId",
                table: "AppStudentFeeDiscounts",
                column: "ApprovedByStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeDiscounts_FeeHeadId",
                table: "AppStudentFeeDiscounts",
                column: "FeeHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeDiscounts_StudentId",
                table: "AppStudentFeeDiscounts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeProfiles_FeeStructureId",
                table: "AppStudentFeeProfiles",
                column: "FeeStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeProfiles_StudentId",
                table: "AppStudentFeeProfiles",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentMonthlyFeeLines_FeeHeadId",
                table: "AppStudentMonthlyFeeLines",
                column: "FeeHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentMonthlyFeeLines_StudentMonthlyFeeId",
                table: "AppStudentMonthlyFeeLines",
                column: "StudentMonthlyFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentMonthlyFees_StudentId",
                table: "AppStudentMonthlyFees",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_Email",
                table: "AppStudents",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudents_TenantId_AdmissionNo",
                table: "AppStudents",
                columns: new[] { "TenantId", "AdmissionNo" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppSubjects_TenantId_Name",
                table: "AppSubjects",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AppTeacherSubjects_TenantId",
                table: "AppTeacherSubjects",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_AppTeacherSubjects_TenantId_StaffId_Section",
                table: "AppTeacherSubjects",
                columns: new[] { "TenantId", "StaffId", "Section" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictApplications_ClientId",
                table: "OpenIddictApplications",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictScopes_Name",
                table: "OpenIddictScopes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_ConversationId",
                table: "ShopAiActionAudits",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_MessageId",
                table: "ShopAiActionAudits",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId",
                table: "ShopAiActionAudits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_ActionName_CreationTime",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "ActionName", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_ExecutionStatus",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "ExecutionStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiActionAudits_TenantId_UserId_CreationTime",
                table: "ShopAiActionAudits",
                columns: new[] { "TenantId", "UserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId",
                table: "ShopAiConversations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId_Status",
                table: "ShopAiConversations",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiConversations_TenantId_UserId_LastMessageDate",
                table: "ShopAiConversations",
                columns: new[] { "TenantId", "UserId", "LastMessageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_ConversationId",
                table: "ShopAiMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId",
                table: "ShopAiMessages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_ConversationId_CreationTime",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "ConversationId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_DetectedAction",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "DetectedAction" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_Status",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiMessages_TenantId_UserId_CreationTime",
                table: "ShopAiMessages",
                columns: new[] { "TenantId", "UserId", "CreationTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_ConversationId",
                table: "ShopAiPendingActions",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_SourceMessageId",
                table: "ShopAiPendingActions",
                column: "SourceMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId",
                table: "ShopAiPendingActions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_ConversationId_Status",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "ConversationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_ExpiryDate",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopAiPendingActions_TenantId_UserId_Status",
                table: "ShopAiPendingActions",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId",
                table: "ShopBankAccounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_BankName",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "BankName" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_Code",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_IsActive",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankAccounts_TenantId_IsDefault",
                table: "ShopBankAccounts",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_BankAccountId",
                table: "ShopBankTransactions",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId",
                table: "ShopBankTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_BankAccountId",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "BankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_BankAccountId_TransactionDate",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "BankAccountId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_Direction",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "Direction" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceNumber",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceType_ReferenceId",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_ReferenceType_ReferenceId_TransactionType_IsReversal",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId", "TransactionType", "IsReversal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_TransactionDate",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransactions_TenantId_TransactionType",
                table: "ShopBankTransactions",
                columns: new[] { "TenantId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_FromBankAccountId",
                table: "ShopBankTransfers",
                column: "FromBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId",
                table: "ShopBankTransfers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_FromBankAccountId",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "FromBankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_Status",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_ToBankAccountId",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "ToBankAccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferDate",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferNumber",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_TenantId_TransferType",
                table: "ShopBankTransfers",
                columns: new[] { "TenantId", "TransferType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopBankTransfers_ToBankAccountId",
                table: "ShopBankTransfers",
                column: "ToBankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_CashRegisterId",
                table: "ShopCashClosings",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId",
                table: "ShopCashClosings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId_CashRegisterId_BusinessDate",
                table: "ShopCashClosings",
                columns: new[] { "TenantId", "CashRegisterId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashClosings_TenantId_CashRegisterId_Status",
                table: "ShopCashClosings",
                columns: new[] { "TenantId", "CashRegisterId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId",
                table: "ShopCashRegisters",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId_Code",
                table: "ShopCashRegisters",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisters_TenantId_IsDefault",
                table: "ShopCashRegisters",
                columns: new[] { "TenantId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_CashClosingId",
                table: "ShopCashRegisterTransactions",
                column: "CashClosingId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_CashRegisterId",
                table: "ShopCashRegisterTransactions",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId",
                table: "ShopCashRegisterTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_CashRegisterId_TransactionDate",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "CashRegisterId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_ReferenceType_ReferenceId",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_ReferenceType_ReferenceId_TransactionType_Direction",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId", "TransactionType", "Direction" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCashRegisterTransactions_TenantId_TransactionDate",
                table: "ShopCashRegisterTransactions",
                columns: new[] { "TenantId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPaymentAllocations_CustomerPaymentId_SaleId",
                table: "ShopCustomerPaymentAllocations",
                columns: new[] { "CustomerPaymentId", "SaleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPaymentAllocations_SaleId",
                table: "ShopCustomerPaymentAllocations",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPaymentAllocations_TenantId",
                table: "ShopCustomerPaymentAllocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPaymentAllocations_TenantId_CustomerPaymentId",
                table: "ShopCustomerPaymentAllocations",
                columns: new[] { "TenantId", "CustomerPaymentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPaymentAllocations_TenantId_SaleId",
                table: "ShopCustomerPaymentAllocations",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_CustomerId",
                table: "ShopCustomerPayments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId",
                table: "ShopCustomerPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_CustomerId",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_CustomerId_PaymentDate",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "CustomerId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_PaymentDate",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_PaymentNumber",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "PaymentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomerPayments_TenantId_Status",
                table: "ShopCustomerPayments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId",
                table: "ShopCustomers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_City",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_Code",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_Country",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_CustomerType",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "CustomerType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_IsActive",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_IsWalkInCustomer",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "IsWalkInCustomer" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_Name",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopCustomers_TenantId_Phone",
                table: "ShopCustomers",
                columns: new[] { "TenantId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopDocumentSequences_TenantId_DocumentType",
                table: "ShopDocumentSequences",
                columns: new[] { "TenantId", "DocumentType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenseCategories_TenantId",
                table: "ShopExpenseCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenseCategories_TenantId_Code",
                table: "ShopExpenseCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenseCategories_TenantId_IsActive",
                table: "ShopExpenseCategories",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenseCategories_TenantId_Name",
                table: "ShopExpenseCategories",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_ExpenseCategoryId",
                table: "ShopExpenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId",
                table: "ShopExpenses",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseCategoryId",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ExpenseCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseCategoryId_ExpenseDate",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ExpenseCategoryId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseDate",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ExpenseNumber",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ExpenseNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_PaidTo",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "PaidTo" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_PaymentMethod",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "PaymentMethod" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_ReferenceNumber",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "ReferenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_Status",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopExpenses_TenantId_Status_ExpenseDate",
                table: "ShopExpenses",
                columns: new[] { "TenantId", "Status", "ExpenseDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_GoodsReceiptId_PurchaseOrderItemId",
                table: "ShopGoodsReceiptItems",
                columns: new[] { "GoodsReceiptId", "PurchaseOrderItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_ProductId",
                table: "ShopGoodsReceiptItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_PurchaseOrderItemId",
                table: "ShopGoodsReceiptItems",
                column: "PurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_TenantId",
                table: "ShopGoodsReceiptItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_TenantId_GoodsReceiptId",
                table: "ShopGoodsReceiptItems",
                columns: new[] { "TenantId", "GoodsReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_TenantId_ProductId",
                table: "ShopGoodsReceiptItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceiptItems_TenantId_PurchaseOrderItemId",
                table: "ShopGoodsReceiptItems",
                columns: new[] { "TenantId", "PurchaseOrderItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_PurchaseOrderId",
                table: "ShopGoodsReceipts",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_SupplierId",
                table: "ShopGoodsReceipts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId",
                table: "ShopGoodsReceipts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_GoodsReceiptNumber",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "GoodsReceiptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_PurchaseOrderId",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_ReceiptDate",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_Status",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_Status_ReceiptDate",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "Status", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_SupplierId",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_SupplierId_ReceiptDate",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "SupplierId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopGoodsReceipts_TenantId_SupplierInvoiceNumber",
                table: "ShopGoodsReceipts",
                columns: new[] { "TenantId", "SupplierInvoiceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId",
                table: "ShopNotifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_ExpiresDate",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "ExpiresDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_Severity_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_SourceKey",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "SourceKey" },
                unique: true,
                filter: "[Status] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_TriggeredDate",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "TriggeredDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_Type_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "Type", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotifications_TenantId_UserId_Status",
                table: "ShopNotifications",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationSettingsList_TenantId",
                table: "ShopNotificationSettingsList",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_NotificationId",
                table: "ShopNotificationUserStates",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId",
                table: "ShopNotificationUserStates",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId_NotificationId_UserId",
                table: "ShopNotificationUserStates",
                columns: new[] { "TenantId", "NotificationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopNotificationUserStates_TenantId_UserId_Status",
                table: "ShopNotificationUserStates",
                columns: new[] { "TenantId", "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPrintSettings_TenantId",
                table: "ShopPrintSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_GoodsReceiptId",
                table: "ShopProductBatches",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_GoodsReceiptItemId",
                table: "ShopProductBatches",
                column: "GoodsReceiptItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_ProductId",
                table: "ShopProductBatches",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_SupplierId",
                table: "ShopProductBatches",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId",
                table: "ShopProductBatches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_AvailableQuantity",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "AvailableQuantity" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ExpiryDate",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ExpiryDate_Status",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ExpiryDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ProductId",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_ProductId_NormalizedBatchNumber",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "ProductId", "NormalizedBatchNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_Status",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductBatches_TenantId_SupplierId",
                table: "ShopProductBatches",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_ParentCategoryId",
                table: "ShopProductCategories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId",
                table: "ShopProductCategories",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId_Code",
                table: "ShopProductCategories",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId_IsActive",
                table: "ShopProductCategories",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId_Name",
                table: "ShopProductCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[ParentCategoryId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId_ParentCategoryId",
                table: "ShopProductCategories",
                columns: new[] { "TenantId", "ParentCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProductCategories_TenantId_ParentCategoryId_Name",
                table: "ShopProductCategories",
                columns: new[] { "TenantId", "ParentCategoryId", "Name" },
                unique: true,
                filter: "[ParentCategoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_CategoryId",
                table: "ShopProducts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId",
                table: "ShopProducts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_Barcode",
                table: "ShopProducts",
                columns: new[] { "TenantId", "Barcode" },
                unique: true,
                filter: "[Barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_CategoryId",
                table: "ShopProducts",
                columns: new[] { "TenantId", "CategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_Code",
                table: "ShopProducts",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_CurrentStock",
                table: "ShopProducts",
                columns: new[] { "TenantId", "CurrentStock" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_IsActive",
                table: "ShopProducts",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_Name",
                table: "ShopProducts",
                columns: new[] { "TenantId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_SKU",
                table: "ShopProducts",
                columns: new[] { "TenantId", "SKU" },
                unique: true,
                filter: "[SKU] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_TenantId_UnitId",
                table: "ShopProducts",
                columns: new[] { "TenantId", "UnitId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopProducts_UnitId",
                table: "ShopProducts",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrderItems_ProductId",
                table: "ShopPurchaseOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrderItems_PurchaseOrderId_ProductId",
                table: "ShopPurchaseOrderItems",
                columns: new[] { "PurchaseOrderId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrderItems_TenantId",
                table: "ShopPurchaseOrderItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrderItems_TenantId_ProductId",
                table: "ShopPurchaseOrderItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrderItems_TenantId_PurchaseOrderId",
                table: "ShopPurchaseOrderItems",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_SupplierId",
                table: "ShopPurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId",
                table: "ShopPurchaseOrders",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId_ExpectedDeliveryDate",
                table: "ShopPurchaseOrders",
                columns: new[] { "TenantId", "ExpectedDeliveryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId_OrderDate",
                table: "ShopPurchaseOrders",
                columns: new[] { "TenantId", "OrderDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId_PurchaseOrderNumber",
                table: "ShopPurchaseOrders",
                columns: new[] { "TenantId", "PurchaseOrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId_Status",
                table: "ShopPurchaseOrders",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseOrders_TenantId_SupplierId",
                table: "ShopPurchaseOrders",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturnItems_GoodsReceiptItemId",
                table: "ShopPurchaseReturnItems",
                column: "GoodsReceiptItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturnItems_ProductId",
                table: "ShopPurchaseReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturnItems_PurchaseReturnId_GoodsReceiptItemId",
                table: "ShopPurchaseReturnItems",
                columns: new[] { "PurchaseReturnId", "GoodsReceiptItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_GoodsReceiptId",
                table: "ShopPurchaseReturns",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_SupplierId",
                table: "ShopPurchaseReturns",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_TenantId_GoodsReceiptId",
                table: "ShopPurchaseReturns",
                columns: new[] { "TenantId", "GoodsReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_TenantId_PurchaseReturnNumber",
                table: "ShopPurchaseReturns",
                columns: new[] { "TenantId", "PurchaseReturnNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_TenantId_ReturnDate",
                table: "ShopPurchaseReturns",
                columns: new[] { "TenantId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_TenantId_Status",
                table: "ShopPurchaseReturns",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopPurchaseReturns_TenantId_SupplierId",
                table: "ShopPurchaseReturns",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_ProductId",
                table: "ShopSaleItemBatchAllocations",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_SaleId",
                table: "ShopSaleItemBatchAllocations",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_SaleItemId_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "SaleItemId", "ProductBatchId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId",
                table: "ShopSaleItemBatchAllocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_ProductBatchId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "ProductBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_SaleId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItemBatchAllocations_TenantId_SaleItemId",
                table: "ShopSaleItemBatchAllocations",
                columns: new[] { "TenantId", "SaleItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItems_ProductId",
                table: "ShopSaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItems_SaleId_ProductId",
                table: "ShopSaleItems",
                columns: new[] { "SaleId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItems_TenantId",
                table: "ShopSaleItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItems_TenantId_ProductId",
                table: "ShopSaleItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleItems_TenantId_SaleId",
                table: "ShopSaleItems",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_ProductId",
                table: "ShopSaleReturnItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_SaleItemId",
                table: "ShopSaleReturnItems",
                column: "SaleItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_SaleReturnId_SaleItemId",
                table: "ShopSaleReturnItems",
                columns: new[] { "SaleReturnId", "SaleItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturnItems_TenantId_SaleReturnId",
                table: "ShopSaleReturnItems",
                columns: new[] { "TenantId", "SaleReturnId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_CustomerId",
                table: "ShopSaleReturns",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_SaleId",
                table: "ShopSaleReturns",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId",
                table: "ShopSaleReturns",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_CustomerId",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_ReturnDate",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "ReturnDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_SaleId",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "SaleId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_SaleReturnNumber",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "SaleReturnNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSaleReturns_TenantId_Status",
                table: "ShopSaleReturns",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_CustomerId",
                table: "ShopSales",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId",
                table: "ShopSales",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_CustomerId",
                table: "ShopSales",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_CustomerId_SaleDate",
                table: "ShopSales",
                columns: new[] { "TenantId", "CustomerId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_SaleDate",
                table: "ShopSales",
                columns: new[] { "TenantId", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_SaleNumber",
                table: "ShopSales",
                columns: new[] { "TenantId", "SaleNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_SaleType",
                table: "ShopSales",
                columns: new[] { "TenantId", "SaleType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_Status",
                table: "ShopSales",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSales_TenantId_Status_SaleDate",
                table: "ShopSales",
                columns: new[] { "TenantId", "Status", "SaleDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSettings_TenantId",
                table: "ShopSettings",
                column: "TenantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_ProductBatchId",
                table: "ShopStockAdjustmentItems",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_ProductId",
                table: "ShopStockAdjustmentItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_StockAdjustmentId_ProductId_ProductBatchId",
                table: "ShopStockAdjustmentItems",
                columns: new[] { "StockAdjustmentId", "ProductId", "ProductBatchId" },
                unique: true,
                filter: "[ProductBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_TenantId",
                table: "ShopStockAdjustmentItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_TenantId_ProductId",
                table: "ShopStockAdjustmentItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustmentItems_TenantId_StockAdjustmentId",
                table: "ShopStockAdjustmentItems",
                columns: new[] { "TenantId", "StockAdjustmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustments_TenantId",
                table: "ShopStockAdjustments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustments_TenantId_AdjustmentDate",
                table: "ShopStockAdjustments",
                columns: new[] { "TenantId", "AdjustmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustments_TenantId_AdjustmentNumber",
                table: "ShopStockAdjustments",
                columns: new[] { "TenantId", "AdjustmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustments_TenantId_Reason",
                table: "ShopStockAdjustments",
                columns: new[] { "TenantId", "Reason" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockAdjustments_TenantId_Status",
                table: "ShopStockAdjustments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_ProductBatchId",
                table: "ShopStockCountItems",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_ProductId",
                table: "ShopStockCountItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_StockCountId_ProductId_ProductBatchId",
                table: "ShopStockCountItems",
                columns: new[] { "StockCountId", "ProductId", "ProductBatchId" },
                unique: true,
                filter: "[ProductBatchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId",
                table: "ShopStockCountItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId_ProductId",
                table: "ShopStockCountItems",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCountItems_TenantId_StockCountId",
                table: "ShopStockCountItems",
                columns: new[] { "TenantId", "StockCountId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_GeneratedStockAdjustmentId",
                table: "ShopStockCounts",
                column: "GeneratedStockAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_ProductCategoryId",
                table: "ShopStockCounts",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId",
                table: "ShopStockCounts",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_CountDate",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "CountDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_GeneratedStockAdjustmentId",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "GeneratedStockAdjustmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_ProductCategoryId",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "ProductCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_Scope",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "Scope" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_Status",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockCounts_TenantId_StockCountNumber",
                table: "ShopStockCounts",
                columns: new[] { "TenantId", "StockCountNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_ProductBatchId",
                table: "ShopStockTransactions",
                column: "ProductBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_ProductId",
                table: "ShopStockTransactions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId",
                table: "ShopStockTransactions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductBatchId",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ProductBatchId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductId",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ProductId_TransactionDate",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ProductId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ReferenceNumber",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ReferenceNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ReferenceType_ReferenceId",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_ReferenceType_SourceItemId",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "ReferenceType", "SourceItemId" },
                unique: true,
                filter: "[SourceItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_TransactionDate",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_TransactionType",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "TransactionType" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopStockTransactions_TenantId_TransactionType_TransactionDate",
                table: "ShopStockTransactions",
                columns: new[] { "TenantId", "TransactionType", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPaymentAllocations_GoodsReceiptId",
                table: "ShopSupplierPaymentAllocations",
                column: "GoodsReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPaymentAllocations_SupplierPaymentId_GoodsReceiptId",
                table: "ShopSupplierPaymentAllocations",
                columns: new[] { "SupplierPaymentId", "GoodsReceiptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPaymentAllocations_TenantId",
                table: "ShopSupplierPaymentAllocations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPaymentAllocations_TenantId_GoodsReceiptId",
                table: "ShopSupplierPaymentAllocations",
                columns: new[] { "TenantId", "GoodsReceiptId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPaymentAllocations_TenantId_SupplierPaymentId",
                table: "ShopSupplierPaymentAllocations",
                columns: new[] { "TenantId", "SupplierPaymentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_SupplierId",
                table: "ShopSupplierPayments",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId",
                table: "ShopSupplierPayments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_PaymentDate",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_PaymentNumber",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "PaymentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_Status",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_SupplierId",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSupplierPayments_TenantId_SupplierId_PaymentDate",
                table: "ShopSupplierPayments",
                columns: new[] { "TenantId", "SupplierId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId",
                table: "ShopSuppliers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_City",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_Code",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_Country",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_Email",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "Email" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_IsActive",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_Name",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_Phone",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopSuppliers_TenantId_TaxNumber",
                table: "ShopSuppliers",
                columns: new[] { "TenantId", "TaxNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopUnits_TenantId",
                table: "ShopUnits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ShopUnits_TenantId_IsActive",
                table: "ShopUnits",
                columns: new[] { "TenantId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ShopUnits_TenantId_Name",
                table: "ShopUnits",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShopUnits_TenantId_ShortName",
                table: "ShopUnits",
                columns: new[] { "TenantId", "ShortName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbpAuditLogActions");

            migrationBuilder.DropTable(
                name: "AbpBackgroundJobs");

            migrationBuilder.DropTable(
                name: "AbpBlobs");

            migrationBuilder.DropTable(
                name: "AbpClaimTypes");

            migrationBuilder.DropTable(
                name: "AbpEntityPropertyChanges");

            migrationBuilder.DropTable(
                name: "AbpFeatureGroups");

            migrationBuilder.DropTable(
                name: "AbpFeatures");

            migrationBuilder.DropTable(
                name: "AbpFeatureValues");

            migrationBuilder.DropTable(
                name: "AbpLinkUsers");

            migrationBuilder.DropTable(
                name: "AbpOrganizationUnitRoles");

            migrationBuilder.DropTable(
                name: "AbpPermissionGrants");

            migrationBuilder.DropTable(
                name: "AbpPermissionGroups");

            migrationBuilder.DropTable(
                name: "AbpPermissions");

            migrationBuilder.DropTable(
                name: "AbpRoleClaims");

            migrationBuilder.DropTable(
                name: "AbpSecurityLogs");

            migrationBuilder.DropTable(
                name: "AbpSessions");

            migrationBuilder.DropTable(
                name: "AbpSettingDefinitions");

            migrationBuilder.DropTable(
                name: "AbpSettings");

            migrationBuilder.DropTable(
                name: "AbpTenantConnectionStrings");

            migrationBuilder.DropTable(
                name: "AbpUserClaims");

            migrationBuilder.DropTable(
                name: "AbpUserDelegations");

            migrationBuilder.DropTable(
                name: "AbpUserLogins");

            migrationBuilder.DropTable(
                name: "AbpUserOrganizationUnits");

            migrationBuilder.DropTable(
                name: "AbpUserRoles");

            migrationBuilder.DropTable(
                name: "AbpUserTokens");

            migrationBuilder.DropTable(
                name: "AppExpenseEntries");

            migrationBuilder.DropTable(
                name: "AppFeeStructureItems");

            migrationBuilder.DropTable(
                name: "AppLateFeePolicies");

            migrationBuilder.DropTable(
                name: "AppStaffAttendances");

            migrationBuilder.DropTable(
                name: "AppStaffDocuments");

            migrationBuilder.DropTable(
                name: "AppStaffSalaryPayments");

            migrationBuilder.DropTable(
                name: "AppStudentAttendances");

            migrationBuilder.DropTable(
                name: "AppStudentDocuments");

            migrationBuilder.DropTable(
                name: "AppStudentFeeDiscounts");

            migrationBuilder.DropTable(
                name: "AppStudentFeeProfiles");

            migrationBuilder.DropTable(
                name: "AppStudentMonthlyFeeLines");

            migrationBuilder.DropTable(
                name: "AppSubjects");

            migrationBuilder.DropTable(
                name: "AppTeacherSubjects");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "ShopAiActionAudits");

            migrationBuilder.DropTable(
                name: "ShopAiPendingActions");

            migrationBuilder.DropTable(
                name: "ShopBankTransactions");

            migrationBuilder.DropTable(
                name: "ShopBankTransfers");

            migrationBuilder.DropTable(
                name: "ShopCashRegisterTransactions");

            migrationBuilder.DropTable(
                name: "ShopCustomerPaymentAllocations");

            migrationBuilder.DropTable(
                name: "ShopDocumentSequences");

            migrationBuilder.DropTable(
                name: "ShopExpenses");

            migrationBuilder.DropTable(
                name: "ShopNotificationSettingsList");

            migrationBuilder.DropTable(
                name: "ShopNotificationUserStates");

            migrationBuilder.DropTable(
                name: "ShopPrintSettings");

            migrationBuilder.DropTable(
                name: "ShopPurchaseReturnItems");

            migrationBuilder.DropTable(
                name: "ShopSaleItemBatchAllocations");

            migrationBuilder.DropTable(
                name: "ShopSaleReturnItems");

            migrationBuilder.DropTable(
                name: "ShopSettings");

            migrationBuilder.DropTable(
                name: "ShopStockAdjustmentItems");

            migrationBuilder.DropTable(
                name: "ShopStockCountItems");

            migrationBuilder.DropTable(
                name: "ShopStockTransactions");

            migrationBuilder.DropTable(
                name: "ShopSupplierPaymentAllocations");

            migrationBuilder.DropTable(
                name: "AbpBlobContainers");

            migrationBuilder.DropTable(
                name: "AbpEntityChanges");

            migrationBuilder.DropTable(
                name: "AbpTenants");

            migrationBuilder.DropTable(
                name: "AbpOrganizationUnits");

            migrationBuilder.DropTable(
                name: "AbpRoles");

            migrationBuilder.DropTable(
                name: "AbpUsers");

            migrationBuilder.DropTable(
                name: "AppExpenseCategories");

            migrationBuilder.DropTable(
                name: "AppStaffs");

            migrationBuilder.DropTable(
                name: "AppFeeStructures");

            migrationBuilder.DropTable(
                name: "AppFeeHeads");

            migrationBuilder.DropTable(
                name: "AppStudentMonthlyFees");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "ShopAiMessages");

            migrationBuilder.DropTable(
                name: "ShopBankAccounts");

            migrationBuilder.DropTable(
                name: "ShopCashClosings");

            migrationBuilder.DropTable(
                name: "ShopCustomerPayments");

            migrationBuilder.DropTable(
                name: "ShopExpenseCategories");

            migrationBuilder.DropTable(
                name: "ShopNotifications");

            migrationBuilder.DropTable(
                name: "ShopPurchaseReturns");

            migrationBuilder.DropTable(
                name: "ShopSaleItems");

            migrationBuilder.DropTable(
                name: "ShopSaleReturns");

            migrationBuilder.DropTable(
                name: "ShopStockCounts");

            migrationBuilder.DropTable(
                name: "ShopProductBatches");

            migrationBuilder.DropTable(
                name: "ShopSupplierPayments");

            migrationBuilder.DropTable(
                name: "AbpAuditLogs");

            migrationBuilder.DropTable(
                name: "AppStudents");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");

            migrationBuilder.DropTable(
                name: "ShopAiConversations");

            migrationBuilder.DropTable(
                name: "ShopCashRegisters");

            migrationBuilder.DropTable(
                name: "ShopSales");

            migrationBuilder.DropTable(
                name: "ShopStockAdjustments");

            migrationBuilder.DropTable(
                name: "ShopGoodsReceiptItems");

            migrationBuilder.DropTable(
                name: "ShopCustomers");

            migrationBuilder.DropTable(
                name: "ShopGoodsReceipts");

            migrationBuilder.DropTable(
                name: "ShopPurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "ShopProducts");

            migrationBuilder.DropTable(
                name: "ShopPurchaseOrders");

            migrationBuilder.DropTable(
                name: "ShopProductCategories");

            migrationBuilder.DropTable(
                name: "ShopUnits");

            migrationBuilder.DropTable(
                name: "ShopSuppliers");
        }
    }
}
