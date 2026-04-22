using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class AddEntryLimitInExpenseCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntryLimitType",
                table: "AppExpenseCategories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryLimitType",
                table: "AppExpenseCategories");
        }
    }
}
