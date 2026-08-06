using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.StockTransactions;

public class ShopStockTransactionPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopStockTransaction, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopStockTransactionPrintMapper(
        IRepository<ShopStockTransaction, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.StockTransaction;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Product!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopStockTransaction), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.ReferenceNumber,
            DocumentDate = entity.TransactionDate,
            Business = business,
            Lines = new()
            {
                new ShopPrintLineDto
                {
                    ProductName = entity.Product?.Name ?? string.Empty,
                    ItemCode = (print?.PrintItemCode ?? false) ? entity.Product?.Code : null,
                    Quantity = entity.QuantityIn != 0 ? entity.QuantityIn : entity.QuantityOut,
                    UnitPrice = entity.UnitCost,
                    LineTotal = entity.TotalCost,
                    BatchNumber = (print?.PrintBatchNumber ?? true) ? entity.BatchNumber : null,
                    ExpiryDate = (print?.PrintExpiryDate ?? true) ? entity.ExpiryDate : null
                }
            },
            Totals = new ShopPrintTotalsDto { NetAmount = entity.TotalCost, PendingAmount = entity.BalanceQuantity },
            Notes = $"{entity.TransactionType} ({entity.ReferenceType})" + (string.IsNullOrWhiteSpace(entity.Notes) ? "" : $" - {entity.Notes}"),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
