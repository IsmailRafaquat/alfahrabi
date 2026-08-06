using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockCounts;

// "Physical Stock Count Summary" per spec - a summary of counted/difference products, not a
// per-item receipt (a full count can have hundreds of lines, unsuitable for thermal paper), so
// Lines[] stays empty and the summary numbers ride in Notes/Totals instead.
public class ShopStockCountPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopStockCount, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopStockCountPrintMapper(
        IRepository<ShopStockCount, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.StockCount;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Items);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopStockCount), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        var counted = entity.Items.Count(x => x.IsCounted);
        var different = entity.Items.Count(x => x.IsCounted && x.DifferenceQuantity != 0);
        var increaseQty = entity.Items.Where(x => x.DifferenceQuantity > 0).Sum(x => x.DifferenceQuantity);
        var decreaseQty = entity.Items.Where(x => x.DifferenceQuantity < 0).Sum(x => -x.DifferenceQuantity);

        var notesParts = new[]
        {
            $"Total Products: {entity.Items.Count}",
            $"Counted Products: {counted}",
            $"Difference Products: {different}",
            $"Increase Quantity: {increaseQty:0.##}",
            $"Decrease Quantity: {decreaseQty:0.##}",
            entity.GeneratedStockAdjustmentId.HasValue ? "Generated Stock Adjustment: Yes" : null,
            entity.Notes
        }.Where(x => !string.IsNullOrWhiteSpace(x));

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.StockCountNumber,
            DocumentDate = entity.CountDate,
            Business = business,
            Lines = new(),
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Items.Count },
            Notes = string.Join(" | ", notesParts),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
