using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;

namespace EHub.ShopManagement.CustomerLedger;

// Exception to the "never call a sibling AppService" rule the other 17 mappers follow: ledger
// balance computation (opening-balance carry-forward across sales/returns/payments) is
// non-trivial domain logic already implemented once in ShopCustomerLedgerAppService. Duplicating
// it here would risk the print statement's balance silently drifting from the on-screen ledger's
// balance - a correctness risk that outweighs the double-permission-check concern for this one
// document type (printing a customer's statement plausibly should require ledger view rights
// anyway, unlike e.g. printing a single sale receipt which shouldn't require full sales access).
//
// documentId is the CustomerId. This generic single-id entry point always prints the full
// history (no date range) - the existing Customer Ledger page keeps its own richer date-range
// statement UI for cases where a bounded period is needed.
public class ShopCustomerLedgerStatementPrintMapper : IShopPrintDocumentMapper
{
    private readonly IShopCustomerLedgerAppService _ledgerAppService;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopCustomerLedgerStatementPrintMapper(
        IShopCustomerLedgerAppService ledgerAppService,
        IShopPrintContextProvider contextProvider)
    {
        _ledgerAppService = ledgerAppService;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.CustomerLedgerStatement;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var statement = await _ledgerAppService.GetStatementAsync(new GetShopCustomerLedgerInput { CustomerId = documentId });

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = statement.CustomerCode,
            DocumentDate = statement.GeneratedDate,
            Business = business,
            Party = new ShopPrintPartyDto
            {
                Label = "Customer",
                Name = statement.CustomerName,
                Phone = statement.CustomerPhone,
                Address = statement.CustomerAddress
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
                PendingAmount = statement.ReceivableAmount,
                AdjustmentAmount = statement.AdvanceAmount
            },
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
