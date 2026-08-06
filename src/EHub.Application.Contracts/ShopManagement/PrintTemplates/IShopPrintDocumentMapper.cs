using System;
using System.Threading.Tasks;

namespace EHub.ShopManagement.PrintTemplates;

// One implementation per document type, each living inside that document's own feature folder
// (e.g. ShopSalePrintMapper next to ShopSaleAppService) so every module owns its own print view
// model instead of one central service knowing the internals of 18 different entities.
//
// Implementations MUST query their repository directly (never call a sibling AppService's
// GetAsync) - ABP's [Authorize] on the sibling AppService would additionally require that
// feature's own Default/view permission, defeating the point of a separate, narrower print
// permission (e.g. a cashier who can print sale receipts but shouldn't browse full sale history).
public interface IShopPrintDocumentMapper
{
    string DocumentType { get; }
    Task<ShopPrintDocumentDto> MapAsync(Guid documentId);
}
