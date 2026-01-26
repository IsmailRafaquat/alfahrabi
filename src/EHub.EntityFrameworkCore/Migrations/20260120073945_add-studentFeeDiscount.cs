using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class addstudentFeeDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStudentFeeDiscounts");
        }
    }
}
