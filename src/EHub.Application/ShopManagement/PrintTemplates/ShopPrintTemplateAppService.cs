using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.PrintTemplates;

[Authorize]
public class ShopPrintTemplateAppService : ApplicationService, IShopPrintTemplateService
{
    // documentType -> permission required to print it. Cash Opening/Closing Slip and Bank
    // Transaction/Transfer Receipt are gated by Payments (no dedicated Cash/Bank leaf exists in
    // the permission list this was built against - confirmed with the feature owner).
    private static readonly Dictionary<string, string> DocumentPermissions = new()
    {
        [ShopPrintDocumentTypes.Sale] = EHubPermissions.ShopPrint.Sales,
        [ShopPrintDocumentTypes.SaleReturn] = EHubPermissions.ShopPrint.Sales,
        [ShopPrintDocumentTypes.PurchaseOrder] = EHubPermissions.ShopPrint.Purchases,
        [ShopPrintDocumentTypes.GoodsReceipt] = EHubPermissions.ShopPrint.Purchases,
        [ShopPrintDocumentTypes.PurchaseReturn] = EHubPermissions.ShopPrint.Purchases,
        [ShopPrintDocumentTypes.CustomerPayment] = EHubPermissions.ShopPrint.Payments,
        [ShopPrintDocumentTypes.SupplierPayment] = EHubPermissions.ShopPrint.Payments,
        [ShopPrintDocumentTypes.CustomerLedgerStatement] = EHubPermissions.ShopPrint.Ledgers,
        [ShopPrintDocumentTypes.SupplierLedgerStatement] = EHubPermissions.ShopPrint.Ledgers,
        [ShopPrintDocumentTypes.ExpenseVoucher] = EHubPermissions.ShopPrint.Expenses,
        [ShopPrintDocumentTypes.CashClosingSlip] = EHubPermissions.ShopPrint.Payments,
        [ShopPrintDocumentTypes.BankTransaction] = EHubPermissions.ShopPrint.Payments,
        [ShopPrintDocumentTypes.BankTransfer] = EHubPermissions.ShopPrint.Payments,
        [ShopPrintDocumentTypes.StockAdjustment] = EHubPermissions.ShopPrint.Inventory,
        [ShopPrintDocumentTypes.StockCount] = EHubPermissions.ShopPrint.Inventory,
        [ShopPrintDocumentTypes.StockTransaction] = EHubPermissions.ShopPrint.Inventory,
        [ShopPrintDocumentTypes.ProductBarcodeLabel] = EHubPermissions.ShopPrint.BarcodeLabels,
        [ShopPrintDocumentTypes.BatchExpiryLabel] = EHubPermissions.ShopPrint.BarcodeLabels
    };

    private readonly Dictionary<string, IShopPrintDocumentMapper> _mappersByType;

    public ShopPrintTemplateAppService(IEnumerable<IShopPrintDocumentMapper> mappers)
    {
        _mappersByType = mappers.ToDictionary(x => x.DocumentType);
    }

    public async Task<ShopPrintDocumentDto> GetPrintDocumentAsync(string documentType, Guid documentId)
    {
        if (string.IsNullOrWhiteSpace(documentType) || !ShopPrintDocumentTypes.All.Contains(documentType))
            throw new BusinessException("ShopManagement:UnknownPrintDocumentType").WithData("documentType", documentType);

        if (!DocumentPermissions.TryGetValue(documentType, out var permission))
            throw new BusinessException("ShopManagement:UnknownPrintDocumentType").WithData("documentType", documentType);

        await AuthorizationService.CheckAsync(permission);

        if (!_mappersByType.TryGetValue(documentType, out var mapper))
            throw new BusinessException("ShopManagement:PrintDocumentTypeNotSupported").WithData("documentType", documentType);

        return await mapper.MapAsync(documentId);
    }
}
