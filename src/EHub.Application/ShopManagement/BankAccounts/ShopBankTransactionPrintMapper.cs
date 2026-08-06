using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.BankAccounts;

public class ShopBankTransactionPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopBankTransaction, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopBankTransactionPrintMapper(
        IRepository<ShopBankTransaction, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.BankTransaction;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.BankAccount!);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopBankTransaction), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = !string.IsNullOrWhiteSpace(entity.ReferenceNumber) ? entity.ReferenceNumber : entity.Id.ToString("N")[..8].ToUpperInvariant(),
            DocumentDate = entity.TransactionDate,
            Business = business,
            Party = entity.BankAccount != null
                ? new ShopPrintPartyDto { Label = "Bank Account", Name = entity.BankAccount.AccountName, ReferenceNumber = ShopPrintMaskingHelper.MaskAccountNumber(entity.BankAccount.AccountNumber) }
                : null,
            Lines = new(),
            Totals = new ShopPrintTotalsDto { NetAmount = entity.Amount, PendingAmount = entity.BalanceAfterTransaction },
            Notes = $"{entity.TransactionType} ({entity.Direction})" + (string.IsNullOrWhiteSpace(entity.Description) ? "" : $" - {entity.Description}"),
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
