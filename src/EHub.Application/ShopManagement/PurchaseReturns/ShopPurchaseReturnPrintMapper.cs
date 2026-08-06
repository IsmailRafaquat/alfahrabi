using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.PurchaseReturns;

public class ShopPurchaseReturnPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopPurchaseReturn, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopPurchaseReturnPrintMapper(
        IRepository<ShopPurchaseReturn, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.PurchaseReturn;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Supplier!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopPurchaseReturn), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.PurchaseReturnNumber,
            DocumentDate = entity.ReturnDate,
            Business = business,
            Party = new ShopPrintPartyDto { Label = "Supplier", Name = entity.Supplier?.Name ?? string.Empty, Phone = entity.Supplier?.Phone },
            Lines = entity.Items.Select(i => new ShopPrintLineDto
            {
                ProductName = i.ProductNameSnapshot,
                ItemCode = (print?.PrintItemCode ?? false) ? i.ProductCodeSnapshot : null,
                Unit = (print?.PrintUnit ?? true) ? i.UnitShortNameSnapshot : null,
                Quantity = i.ReturnQuantity,
                UnitPrice = i.UnitPurchasePrice,
                TaxAmount = (print?.PrintTax ?? true) ? i.TaxAmount : 0,
                LineTotal = i.LineTotal,
                BatchNumber = (print?.PrintBatchNumber ?? true) ? i.BatchNumber : null,
                ExpiryDate = (print?.PrintExpiryDate ?? true) ? i.ExpiryDate : null
            }).ToList(),
            Totals = new ShopPrintTotalsDto
            {
                SubTotal = entity.SubTotal,
                TaxAmount = (print?.PrintTax ?? true) ? entity.TaxAmount : null,
                OtherChargesAmount = entity.OtherCharges != 0 ? entity.OtherCharges : null,
                NetAmount = entity.GrandTotal
            },
            Notes = $"Reason: {entity.Reason}" + (string.IsNullOrWhiteSpace(entity.ReasonDetails) ? "" : $" - {entity.ReasonDetails}")
                + (string.IsNullOrWhiteSpace(entity.Notes) ? "" : $" | {entity.Notes}"),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.PurchaseReturnNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.PurchaseReturnNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
