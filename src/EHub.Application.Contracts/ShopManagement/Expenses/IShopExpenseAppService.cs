using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace EHub.ShopManagement.Expenses;

public interface IShopExpenseAppService : IApplicationService
{
    Task<PagedResultDto<ShopExpenseDto>> GetListAsync(GetShopExpensesInput input);

    Task<ShopExpenseDto> GetAsync(Guid id);

    Task<ShopExpenseDto> CreateAsync(CreateUpdateShopExpenseDto input);

    Task<ShopExpenseDto> UpdateAsync(Guid id, CreateUpdateShopExpenseDto input);

    Task DeleteAsync(Guid id);

    Task<ShopExpenseDto> PostAsync(Guid id);

    Task<ShopExpenseDto> CancelAsync(Guid id, CancelShopExpenseDto input);

    Task<ShopExpenseSummaryDto> GetSummaryAsync(GetShopExpensesInput input);
}
