using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.ProductBatches;

// documentId is the ProductBatchId. Same label shape as ShopProductBarcodeLabelPrintMapper, plus
// batch/expiry fields (which only exist here, on ShopProductBatch, never invented for products
// that don't track batches).
public class ShopBatchExpiryLabelPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopProductBatch, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopBatchExpiryLabelPrintMapper(
        IRepository<ShopProductBatch, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.BatchExpiryLabel;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Product!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopProductBatch), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.BatchNumber,
            DocumentDate = DateTime.Now,
            Business = business,
            Lines = new()
            {
                new ShopPrintLineDto
                {
                    ProductName = entity.Product?.Name ?? string.Empty,
                    ItemCode = entity.Product?.Code,
                    UnitPrice = entity.Product?.SalePrice ?? 0,
                    LineTotal = entity.Product?.SalePrice ?? 0,
                    BatchNumber = entity.BatchNumber,
                    ExpiryDate = entity.ExpiryDate,
                    ManufacturingDate = entity.ManufacturingDate
                }
            },
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Product?.SalePrice ?? 0 },
            BarcodeValue = entity.Product?.Barcode,
            PaperSize = EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal58Mm
        };
    }
}
