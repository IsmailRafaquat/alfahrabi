using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class StudentUpdatation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppEnrollments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AppStudents");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "AppStudents",
                newName: "ECPhone");

            migrationBuilder.AlterColumn<string>(
                name: "AdmissionNo",
                table: "AppStudents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Accommodations",
                table: "AppStudents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "City",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Commnets",
                table: "AppStudents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ECEmail",
                table: "AppStudents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ECFirstName",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ECLastName",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ECRelationShipToStudent",
                table: "AppStudents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extracurrucular",
                table: "AppStudents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grade",
                table: "AppStudents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "GradeLevel",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MedicalConditions",
                table: "AppStudents",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PEmail",
                table: "AppStudents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PFirstName",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PLastName",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PPhone",
                table: "AppStudents",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PRelatonShipToStudent",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PerviousSchool",
                table: "AppStudents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Province",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Section",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Shift",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StreetAddress",
                table: "AppStudents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StreetAddressLine2",
                table: "AppStudents",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentIdNo",
                table: "AppStudents",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Term",
                table: "AppStudents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "AppStudents",
                type: "nvarchar(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Accommodations",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Commnets",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ECEmail",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ECFirstName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ECLastName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ECRelationShipToStudent",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Extracurrucular",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "MedicalConditions",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PEmail",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PFirstName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PLastName",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PPhone",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PRelatonShipToStudent",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "PerviousSchool",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Shift",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "StreetAddress",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "StreetAddressLine2",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "StudentIdNo",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "Term",
                table: "AppStudents");

            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "AppStudents");

            migrationBuilder.RenameColumn(
                name: "ECPhone",
                table: "AppStudents",
                newName: "Phone");

            migrationBuilder.AlterColumn<string>(
                name: "AdmissionNo",
                table: "AppStudents",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AppStudents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AppEnrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicYear = table.Column<DateTime>(type: "date", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeleterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExtraProperties = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Program = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Section = table.Column<int>(type: "int", nullable: true),
                    Shift = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StudentId1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Term = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppEnrollments_AppStudents_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AppStudents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AppEnrollments_AppStudents_StudentId1",
                        column: x => x.StudentId1,
                        principalTable: "AppStudents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_StudentId",
                table: "AppEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_StudentId1",
                table: "AppEnrollments",
                column: "StudentId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_TenantId_Grade_Section_Shift",
                table: "AppEnrollments",
                columns: new[] { "TenantId", "Grade", "Section", "Shift" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_TenantId_Status",
                table: "AppEnrollments",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_TenantId_StudentId",
                table: "AppEnrollments",
                columns: new[] { "TenantId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AppEnrollments_TenantId_StudentId_AcademicYear_Term",
                table: "AppEnrollments",
                columns: new[] { "TenantId", "StudentId", "AcademicYear", "Term" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");
        }
    }
}
