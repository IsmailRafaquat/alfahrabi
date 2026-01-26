using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class addstudentMonthlyFeeLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentMonthlyFeeLines_FeeHeadId",
                table: "AppStudentMonthlyFeeLines",
                column: "FeeHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentMonthlyFeeLines_StudentMonthlyFeeId",
                table: "AppStudentMonthlyFeeLines",
                column: "StudentMonthlyFeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppStudentMonthlyFeeLines");
        }
    }
}
