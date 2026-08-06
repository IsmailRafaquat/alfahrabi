using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.SaleReturns;

public class ShopSaleReturnPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopSaleReturn, Guid> _repository;
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopSaleReturnPrintMapper(
        IRepository<ShopSaleReturn, Guid> repository,
        IRepository<ShopCustomer, Guid> customerRepository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.SaleReturn;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopSaleReturn), documentId);

        var customerQuery = await _customerRepository.GetQueryableAsync();
        var customer = customerQuery.FirstOrDefault(x => x.Id == entity.CustomerId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.SaleReturnNumber,
            DocumentDate = entity.ReturnDate,
            Business = business,
            Party = new ShopPrintPartyDto
            {
                Label = "Customer",
                Name = customer?.Name ?? "Walk-in Customer",
                Phone = customer?.Phone
            },
            Lines = entity.Items.Select(i => new ShopPrintLineDto
            {
                ProductName = i.ProductNameSnapshot,
                ItemCode = (print?.PrintItemCode ?? false) ? i.ProductCodeSnapshot : null,
                Unit = (print?.PrintUnit ?? true) ? i.UnitShortNameSnapshot : null,
                Quantity = i.ReturnQuantity,
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
                PaidAmount = entity.RefundAmount != 0 ? entity.RefundAmount : null,
                AdjustmentAmount = entity.CustomerCreditAmount != 0 ? entity.CustomerCreditAmount : null
            },
            Notes = $"Reason: {entity.Reason}" + (string.IsNullOrWhiteSpace(entity.ReasonDetails) ? "" : $" - {entity.ReasonDetails}")
                + (string.IsNullOrWhiteSpace(entity.Notes) ? "" : $" | {entity.Notes}"),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.SaleReturnNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.SaleReturnNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
