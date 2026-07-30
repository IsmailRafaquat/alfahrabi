using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EHub.ShopManagement.Customers;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace EHub.ShopManagement.AiAssistant;

public class ShopAiLookupResolver : IShopAiLookupResolver, ITransientDependency
{
    private readonly IRepository<ShopCustomer, Guid> _customerRepository;
    private readonly ICurrentTenant _currentTenant;

    public ShopAiLookupResolver(IRepository<ShopCustomer, Guid> customerRepository, ICurrentTenant currentTenant)
    {
        _customerRepository = customerRepository;
        _currentTenant = currentTenant;
    }

    public async Task<ShopAiLookupResult> ResolveCustomerByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var tenantId = _currentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");
        var trimmed = name.Trim();

        var query = await _customerRepository.GetQueryableAsync();
        var tenantCustomers = query.Where(x => x.TenantId == tenantId && x.IsActive);

        // Exact match first.
        var exact = tenantCustomers.Where(x => x.Name == trimmed).Select(x => new { x.Id, x.Name }).ToList();
        if (exact.Count == 1)
        {
            return new ShopAiLookupResult { Id = exact[0].Id, DisplayName = exact[0].Name };
        }
        if (exact.Count > 1)
        {
            return Ambiguous(exact.Select(x => new ShopAiLookupChoiceDto { Id = x.Id, DisplayText = x.Name }));
        }

        // Normalized case-insensitive match second - never a fuzzy/similarity match.
        var normalized = trimmed.ToUpperInvariant();
        var caseInsensitive = tenantCustomers
            .Select(x => new { x.Id, x.Name })
            .ToList()
            .Where(x => x.Name.Trim().ToUpperInvariant() == normalized)
            .ToList();

        if (caseInsensitive.Count == 1)
        {
            return new ShopAiLookupResult { Id = caseInsensitive[0].Id, DisplayName = caseInsensitive[0].Name };
        }
        if (caseInsensitive.Count > 1)
        {
            return Ambiguous(caseInsensitive.Select(x => new ShopAiLookupChoiceDto { Id = x.Id, DisplayText = x.Name }));
        }

        return new ShopAiLookupResult();
    }

    private static ShopAiLookupResult Ambiguous(IEnumerable<ShopAiLookupChoiceDto> choices) =>
        new() { IsAmbiguous = true, Choices = choices.ToList() };
}
