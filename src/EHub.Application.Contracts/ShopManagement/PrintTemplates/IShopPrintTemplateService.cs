using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.PrintTemplates;

public interface IShopPrintTemplateService : IApplicationService
{
    Task<ShopPrintDocumentDto> GetPrintDocumentAsync(string documentType, Guid documentId);
}
