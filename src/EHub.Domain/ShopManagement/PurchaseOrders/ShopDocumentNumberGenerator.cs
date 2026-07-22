using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace EHub.ShopManagement.PurchaseOrders;

/// <summary>
/// Minimal tenant-scoped, concurrency-safe sequential document number generator.
/// Reusable for future document types (e.g. Goods Receipts, Invoices) via <paramref name="documentType"/>.
/// </summary>
public class ShopDocumentNumberGenerator : DomainService
{
    private const int MaxAttempts = 10;

    private readonly IRepository<ShopDocumentSequence, Guid> _repository;

    public ShopDocumentNumberGenerator(IRepository<ShopDocumentSequence, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<string> GetNextNumberAsync(Guid tenantId, string documentType, string prefix, int padding = 6)
    {
        for (var attempt = 0; attempt < MaxAttempts; attempt++)
        {
            var query = await _repository.GetQueryableAsync();
            var sequence = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TenantId == tenantId && x.DocumentType == documentType));

            if (sequence == null)
            {
                sequence = new ShopDocumentSequence(GuidGenerator.Create(), tenantId, documentType);
                try
                {
                    await _repository.InsertAsync(sequence, autoSave: true);
                }
                catch (Exception)
                {
                    // Another request inserted the sequence row concurrently (unique TenantId+DocumentType
                    // constraint); retry to read the row it created.
                    continue;
                }
            }

            var next = sequence.IncrementAndGet();
            try
            {
                await _repository.UpdateAsync(sequence, autoSave: true);
                return prefix + next.ToString().PadLeft(padding, '0');
            }
            catch (Exception)
            {
                // Another request incremented the same sequence row concurrently (ConcurrencyStamp
                // mismatch); retry with a freshly read sequence value.
            }
        }

        throw new BusinessException("ShopManagement:PurchaseOrderNumberGenerationFailed");
    }
}
