using System;
using System.Linq;
using System.Threading.Tasks;
using EHub.ShopManagement.PrintTemplates;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.CustomerPayments;

public class ShopCustomerPaymentPrintMapper : IShopPrintDocumentMapper
{
    private readonly IRepository<ShopCustomerPayment, Guid> _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IShopPrintContextProvider _contextProvider;

    public ShopCustomerPaymentPrintMapper(
        IRepository<ShopCustomerPayment, Guid> repository,
        ICurrentTenant currentTenant,
        IShopPrintContextProvider contextProvider)
    {
        _repository = repository;
        _currentTenant = currentTenant;
        _contextProvider = contextProvider;
    }

    public string DocumentType => ShopPrintDocumentTypes.CustomerPayment;

    public async Task<ShopPrintDocumentDto> MapAsync(Guid documentId)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

        var query = await _repository.WithDetailsAsync(x => x.Customer!, x => x.Allocations);
        var entity = query.FirstOrDefault(x => x.Id == documentId && x.TenantId == tenantId)
            ?? throw new EntityNotFoundException(typeof(ShopCustomerPayment), documentId);

        var context = await _contextProvider.GetContextAsync();
        var business = _contextProvider.BuildBusinessDto(context);
        var print = context.PrintSetting;

        var allocatedAmount = entity.Allocations.Sum(a => a.AllocatedAmount);

        return new ShopPrintDocumentDto
        {
            DocumentType = DocumentType,
            DocumentNumber = entity.PaymentNumber,
            DocumentDate = entity.PaymentDate,
            Business = business,
            Party = new ShopPrintPartyDto { Label = "Customer", Name = entity.Customer?.Name ?? string.Empty, Phone = entity.Customer?.Phone, ReferenceNumber = entity.ReferenceNumber },
            Lines = new(),
            Totals = new ShopPrintTotalsDto
            {
                NetAmount = entity.Amount,
                AdjustmentAmount = allocatedAmount != 0 ? allocatedAmount : null,
                PendingAmount = (entity.Amount - allocatedAmount) != 0 ? entity.Amount - allocatedAmount : null
            },
            Payment = (print?.PrintPaymentDetails ?? true)
                ? new ShopPrintPaymentDto { Method = entity.PaymentMethod.ToString(), ReceivedAmount = entity.Amount, ReferenceNumber = entity.ChequeNumber ?? entity.ReferenceNumber }
                : null,
            Notes = entity.Notes,
            FooterMessage = print?.PrintFooterMessage ?? context.ShopSetting?.ReceiptFooter,
            QrCodeValue = (print?.PrintQrCode ?? false) ? entity.PaymentNumber : null,
            BarcodeValue = (print?.PrintBarcode ?? false) ? entity.PaymentNumber : null,
            PaperSize = print?.DefaultPrintPaperSize ?? EHub.ShopManagement.PrintSettings.ShopPrintPaperSize.Thermal80Mm
        };
    }
}
