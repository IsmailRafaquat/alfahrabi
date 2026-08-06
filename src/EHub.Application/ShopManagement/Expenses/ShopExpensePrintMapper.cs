using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Expenses;

// Representative "flat single-line document" mapper - no Lines[], just Notes + Totals.NetAmount.
public class ShopExpensePrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopExpense, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopExpensePrintMapper(
        IRepository<ShopExpense, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.ExpenseVoucher;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.ExpenseCategory!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopExpense), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        var notesParts = new[]
        {
            entity.ExpenseCategory != null ? $"Category: {entity.ExpenseCategory.Name}" : null,
            !string.IsNullOrWhiteSpace(entity.Description) ? $"Description: {entity.Description}" : null,
            !string.IsNullOrWhiteSpace(entity.BankName) ? $"Bank: {entity.BankName}" : null,
            entity.Notes
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.ExpenseNumber,
            DocumentDate = entity.ExpenseDate,
            Business = business,
            Party = !string.IsNullOrWhiteSpace(entity.PaidTo)
                ? new ShopPrintPartyDto { Label = "Paid To", Name = entity.PaidTo, ReferenceNumber = entity.ReferenceNumber }
                : null,
            Lines = new(),
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Amount },
            Payment = (print?.PrintPaymentDetails ?? true)
                ? new ShopPrintPaymentDto { Method = entity.PaymentMethod.ToString(), ReferenceNumber = entity.ChequeNumber }
                : null,
            Notes = notesParts.Any() ? string.Join(" | ", notesParts) : null,
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.ExpenseNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.ExpenseNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
