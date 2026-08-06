using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockAdjustments;

// Quantity-based document, no monetary totals - Totals.NetAmount carries the item count instead
// so the shared totals component always has something to show, and per-line UnitPrice/LineTotal
// are repurposed to show system/final quantity via the Quantity field only (price fields left 0).
public class ShopStockAdjustmentPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopStockAdjustment, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopStockAdjustmentPrintMapper(
        IRepository<ShopStockAdjustment, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.StockAdjustment;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopStockAdjustment), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.AdjustmentNumber,
            DocumentDate = entity.AdjustmentDate,
            Business = business,
            Lines = entity.Items.Select(i => new ShopPrintLineDto
            {
                ProductName = $"{i.ProductNameSnapshot} ({i.AdjustmentType})",
                ItemCode = (print?.PrintItemCode ?? false) ? i.ProductCodeSnapshot : null,
                Unit = (print?.PrintUnit ?? true) ? i.UnitShortNameSnapshot : null,
                Quantity = i.AdjustmentQuantity,
                UnitPrice = i.SystemQuantitySnapshot,
                LineTotal = i.FinalQuantity,
                BatchNumber = (print?.PrintBatchNumber ?? true) ? i.BatchNumber : null,
                ExpiryDate = (print?.PrintExpiryDate ?? true) ? i.ExpiryDate : null,
                ManufacturingDate = i.ManufacturingDate
            }).ToList(),
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Items.Count },
            Notes = $"Reason: {entity.Reason}" + (string.IsNullOrWhiteSpace(entity.ReasonDetails) ? "" : $" - {entity.ReasonDetails}")
                + (string.IsNullOrWhiteSpace(entity.Notes) ? "" : $" | {entity.Notes}"),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
