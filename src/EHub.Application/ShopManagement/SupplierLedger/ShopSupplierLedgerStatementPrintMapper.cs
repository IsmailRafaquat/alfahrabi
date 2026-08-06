using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;

namespace EHub.ShopManagement.SupplierLedger;

// See ShopCustomerLedgerStatementPrintMapper for why this one mapper calls a sibling AppService
// directly - identical reasoning applies to supplier balance computation. documentId is the
// SupplierId; always prints full history (no date range) via this generic entry point.
public class ShopSupplierLedgerStatementPrintMapper : IShopPrintDocumentMapper
{
    private readonly IShopSupplierLedgerAppService _ledgerAppService;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopSupplierLedgerStatementPrintMapper(
        IShopSupplierLedgerAppService ledgerAppService,
        IShopPrintContextProvider contextProvider)
    {
        _ledgerAppService = ledgerAppService;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.SupplierLedgerStatement;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var statement = await _ledgerAppService.GetStatementAsync(new GetShopSupplierLedgerInput { SupplierId = documentId });

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = statement.SupplierCode,
            DocumentDate = statement.GeneratedDate,
            Business = business,
            Party = new ShopPrintPartyDto
            {
                Label = "Supplier",
                Name = statement.SupplierName,
                Phone = statement.SupplierPhone,
                Address = statement.SupplierAddress
            },
            Lines = statement.Entries.Select(e => new ShopPrintLineDto
            {
                ProductName = $"{e.TransactionDate:dd-MMM-yyyy} {e.ReferenceNumber} - {e.Description}",
                Quantity = 1,
                UnitPrice = e.DebitAmount ?? e.CreditAmount ?? 0,
                LineTotal = e.RunningBalance ?? 0
            }).ToList(),
            Totals = new ShopPrintTotalsDto
            {
                SubTotal = statement.OpeningBalance,
                DiscountAmount = statement.TotalCredit,
                TaxAmount = statement.TotalDebit,
                NetAmount = statement.ClosingBalance ?? 0,
                PendingAmount = statement.PayableAmount,
                AdjustmentAmount = statement.AdvanceAmount
            },
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
