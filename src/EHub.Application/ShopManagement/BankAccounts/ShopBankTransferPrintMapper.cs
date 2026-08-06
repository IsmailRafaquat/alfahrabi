using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransferPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopBankTransfer, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopBankTransferPrintMapper(
        IRepository<ShopBankTransfer, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.BankTransfer;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.FromBankAccount!, x => x.ToBankAccount!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopBankTransfer), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        var fromLabel = entity.FromBankAccount != null ? entity.FromBankAccount.AccountName : "Cash Register";
        var toLabel = entity.ToBankAccount != null ? entity.ToBankAccount.AccountName : "Cash Register";

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.TransferNumber,
            DocumentDate = entity.TransferDate,
            Business = business,
            Party = new ShopPrintPartyDto { Label = "Transfer", Name = $"{fromLabel} -> {toLabel}", ReferenceNumber = entity.ReferenceNumber },
            Lines = new(),
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Amount },
            Notes = entity.Notes,
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
