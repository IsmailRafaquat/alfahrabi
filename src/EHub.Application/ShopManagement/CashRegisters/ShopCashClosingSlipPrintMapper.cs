using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.CashRegisters;

// Serves both the Cash Opening Slip and Cash Closing Slip - both are the same ShopCashClosing
// entity (Opening* fields are always populated; Closing* fields populate once ApplyClosingTotals
// runs). Note lines cover the movement breakdown since there is no monetary Lines[] here.
public class ShopCashClosingSlipPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopCashClosing, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopCashClosingSlipPrintMapper(
        IRepository<ShopCashClosing, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.CashClosingSlip;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.CashRegister!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopCashClosing), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;
        var isClosed = entity.Status == ShopCashClosingStatus.Closed;

        var notesParts = new[]
        {
            $"Cash Sales: {entity.CashSales:0.00}",
            $"Customer Cash Payments: {entity.CustomerCashPayments:0.00}",
            $"Supplier Cash Payments: {entity.SupplierCashPayments:0.00}",
            $"Cash Expenses: {entity.CashExpenses:0.00}",
            entity.ManualCashIn != 0 ? $"Manual Cash In: {entity.ManualCashIn:0.00}" : null,
            entity.ManualCashOut != 0 ? $"Manual Cash Out: {entity.ManualCashOut:0.00}" : null,
            entity.Notes
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.CashRegister != null ? $"{entity.CashRegister.Code}-{entity.BusinessDate:yyyyMMdd}" : entity.Id.ToString("N")[..8].ToUpperInvariant(),
            DocumentDate = entity.BusinessDate,
            Business = business,
            Party = entity.CashRegister != null ? new ShopPrintPartyDto { Label = "Register", Name = entity.CashRegister.Name } : null,
            Lines = new(),
            Totals = new ShopPrintTotalsDto
            {
                SubTotal = entity.OpeningCash,
                NetAmount = isClosed ? entity.ActualClosingCash ?? entity.ExpectedClosingCash : entity.ExpectedClosingCash,
                AdjustmentAmount = isClosed ? entity.DifferenceAmount : null
            },
            Notes = string.Join(" | ", notesParts),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
