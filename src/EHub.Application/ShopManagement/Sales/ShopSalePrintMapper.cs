using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Sales;

// Representative "line-item document" mapper - queries the repository directly (never the
// sibling ShopSaleAppService) so printing is gated purely by ShopManagement.Print.Sales, not
// also by ShopManagement.Sales.Default. See IShopPrintDocumentMapper for why.
public class ShopSalePrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopSale, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopSalePrintMapper(
        IRepository<ShopSale, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.Sale;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items, x => x.Customer!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopSale), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        var dto = new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.SaleNumber,
            DocumentDate = entity.SaleDate,
            Business = business,
            Party = new ShopPrintPartyDto
            {
                Label = "Customer",
                Name = entity.Customer?.Name ?? "Walk-in Customer",
                Phone = entity.Customer?.Phone,
                Address = entity.Customer?.AddressLine1
            },
            Lines = entity.Items.Select(i => new ShopPrintLineDto
            {
                ProductName = i.ProductNameSnapshot,
                ItemCode = (print?.PrintItemCode ?? false) ? i.ProductCodeSnapshot : null,
                Unit = (print?.PrintUnit ?? true) ? i.UnitShortNameSnapshot : null,
                Quantity = i.Quantity,
                UnitPrice = i.UnitSalePrice,
                DiscountAmount = (print?.PrintDiscount ?? true) ? i.DiscountAmount : 0,
                TaxAmount = (print?.PrintTax ?? true) ? i.TaxAmount : 0,
                LineTotal = i.LineTotal,
                BatchNumber = (print?.PrintBatchNumber ?? true) ? i.BatchNumber : null,
                ExpiryDate = (print?.PrintExpiryDate ?? true) ? i.ExpiryDate : null
            }).ToList(),
            Totals = new ShopPrintTotalsDto
            {
                SubTotal = entity.SubTotal,
                DiscountAmount = (print?.PrintDiscount ?? true) ? entity.DiscountAmount : null,
                TaxAmount = (print?.PrintTax ?? true) ? entity.TaxAmount : null,
                OtherChargesAmount = entity.OtherCharges != 0 ? entity.OtherCharges : null,
                NetAmount = entity.GrandTotal,
                PaidAmount = entity.PaidAmount,
                PendingAmount = entity.PendingAmount
            },
            Payment = (print?.PrintPaymentDetails ?? true)
                ? new ShopPrintPaymentDto { Method = entity.PaymentMethod.ToString(), ReceivedAmount = entity.PaidAmount }
                : null,
            Notes = entity.Notes,
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.SaleNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.SaleNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };

        return dto;
    }
}
