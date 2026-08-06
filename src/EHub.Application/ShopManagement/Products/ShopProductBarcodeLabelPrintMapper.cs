using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.Products;

// documentId is the ProductId. This is a label (small fixed layout), not a receipt - Lines stays
// empty and label content (name/barcode/code/price) rides in Notes/BarcodeValue/Totals, rendered
// by the frontend's dedicated label component rather than the receipt/item-table components.
public class ShopProductBarcodeLabelPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopProduct, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopProductBarcodeLabelPrintMapper(
        IRepository<ShopProduct, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.ProductBarcodeLabel;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.GetQueryableAsync();
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopProduct), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.Code,
            DocumentDate = DateTime.Now,
            Business = business,
            Lines = new()
            {
                new ShopPrintLineDto { ProductName = entity.Name, ItemCode = entity.Code, UnitPrice = entity.SalePrice, LineTotal = entity.SalePrice }
            },
            Totals = new ShopPrintTotalsDto { NetAmount = entity.SalePrice },
            BarcodeValue = entity.Barcode,
            PaperSize = EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal58Mm
        };
    }
}
