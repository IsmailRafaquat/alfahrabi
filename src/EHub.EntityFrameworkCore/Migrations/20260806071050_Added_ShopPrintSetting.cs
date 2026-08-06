using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHub.Migrations
{
    /// <inheritdoc />
    public partial class Added_ShopPrintSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_ShopPrintSettings_TenantId",
                table: "ShopPrintSettings",
                column: "TenantId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShopPrintSettings");
        }
    }
}
