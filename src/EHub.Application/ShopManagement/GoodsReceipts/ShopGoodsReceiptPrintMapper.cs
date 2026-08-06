using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.GoodsReceipts;

public class ShopGoodsReceiptPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopGoodsReceipt, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopGoodsReceiptPrintMapper(
        IRepository<ShopGoodsReceipt, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.GoodsReceipt;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Supplier!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopGoodsReceipt), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.GoodsReceiptNumber,
            DocumentDate = entity.ReceiptDate,
            Business = business,
            Party = new ShopPrintPartyDto
            {
                Label = "Supplier",
                Name = entity.Supplier?.Name ?? string.Empty,
                Phone = entity.Supplier?.Phone,
                ReferenceNumber = entity.SupplierInvoiceNumber
            },
            Lines = entity.Items.Select(i => new ShopPrintLineDto
            {
                ProductName = i.ProductNameSnapshot,
                ItemCode = (print?.PrintItemCode ?? false) ? i.ProductCodeSnapshot : null,
                Unit = (print?.PrintUnit ?? true) ? i.UnitShortNameSnapshot : null,
                Quantity = i.ReceivedQuantity + i.BonusQuantity,
                UnitPrice = i.PurchasePrice,
                DiscountAmount = (print?.PrintDiscount ?? true) ? i.DiscountAmount : 0,
                TaxAmount = (print?.PrintTax ?? true) ? i.TaxAmount : 0,
                LineTotal = i.LineTotal,
                BatchNumber = (print?.PrintBatchNumber ?? true) ? i.BatchNumber : null,
                ExpiryDate = (print?.PrintExpiryDate ?? true) ? i.ExpiryDate : null,
                ManufacturingDate = i.ManufacturingDate
            }).ToList(),
            Totals = new ShopPrintTotalsDto
            {
                SubTotal = entity.SubTotal,
                DiscountAmount = (print?.PrintDiscount ?? true) ? entity.DiscountAmount : null,
                TaxAmount = (print?.PrintTax ?? true) ? entity.TaxAmount : null,
                OtherChargesAmount = (entity.ShippingCharges + entity.OtherCharges) != 0 ? entity.ShippingCharges + entity.OtherCharges : null,
                NetAmount = entity.GrandTotal
            },
            Notes = entity.Notes,
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.GoodsReceiptNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.GoodsReceiptNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
