using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class initial1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppStudentFeeProfiles_FeeStructureId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AppStudentFeeProfiles_StudentId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropColumn(
                name: "FeeStructureId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropColumn(
                name: "StudentId1",
                table: "AppStudentFeeProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId",
                table: "AppStudentFeeProfiles",
                column: "FeeStructureId",
                principalTable: "AppFeeStructures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId",
                table: "AppStudentFeeProfiles",
                column: "StudentId",
                principalTable: "AppStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId",
                table: "AppStudentFeeProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId",
                table: "AppStudentFeeProfiles");

            migrationBuilder.AddColumn<Guid>(
                name: "FeeStructureId1",
                table: "AppStudentFeeProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId1",
                table: "AppStudentFeeProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeProfiles_FeeStructureId1",
                table: "AppStudentFeeProfiles",
                column: "FeeStructureId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppStudentFeeProfiles_StudentId1",
                table: "AppStudentFeeProfiles",
                column: "StudentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId",
                table: "AppStudentFeeProfiles",
                column: "FeeStructureId",
                principalTable: "AppFeeStructures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppFeeStructures_FeeStructureId1",
                table: "AppStudentFeeProfiles",
                column: "FeeStructureId1",
                principalTable: "AppFeeStructures",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId",
                table: "AppStudentFeeProfiles",
                column: "StudentId",
                principalTable: "AppStudents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AppStudentFeeProfiles_AppStudents_StudentId1",
                table: "AppStudentFeeProfiles",
                column: "StudentId1",
                principalTable: "AppStudents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
