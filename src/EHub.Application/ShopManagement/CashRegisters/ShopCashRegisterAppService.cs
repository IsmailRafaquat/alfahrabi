using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using EHub.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EHub.ShopManagement.CashRegisters;

public class ShopCashRegisterAppService : ApplicationService, IShopCashRegisterAppService
{
    private readonly IRepository<ShopCashRegister, Guid> _registerRepository;
    private readonly IRepository<ShopCashRegisterTransaction, Guid> _transactionRepository;
    private readonly IRepository<ShopCashClosing, Guid> _closingRepository;
    private readonly ShopCashRegisterManager _manager;

    public ShopCashRegisterAppService(
        IRepository<ShopCashRegister, Guid> registerRepository,
        IRepository<ShopCashRegisterTransaction, Guid> transactionRepository,
        IRepository<ShopCashClosing, Guid> closingRepository,
        ShopCashRegisterManager manager)
    {
        _registerRepository = registerRepository;
        _transactionRepository = transactionRepository;
        _closingRepository = closingRepository;
        _manager = manager;
    }

    // ---------------------------------------------------------------------
    // Register CRUD
    // ---------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopCashRegisters.Default)]
    public async Task<PagedResultDto<ShopCashRegisterDto>> GetListAsync(GetShopCashRegistersInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopCashRegisterDto>(0, new List<ShopCashRegisterDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _registerRepository.GetQueryableAsync();
        query = query.Where(x => x.TenantId == tenantId)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x => x.Code.Contains(input.Filter!) || x.Name.Contains(input.Filter!))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "Name asc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).PageBy(input));
        var items = ObjectMapper.Map<List<ShopCashRegister>, List<ShopCashRegisterDto>>(entities);
        return new PagedResultDto<ShopCashRegisterDto>(totalCount, items);
    }

    [Authorize(EHubPermissions.ShopCashRegisters.Default)]
    public async Task<ShopCashRegisterDto> GetAsync(Guid id)
    {
        var entity = await FindRegisterAsync(id);
        return ObjectMapper.Map<ShopCashRegister, ShopCashRegisterDto>(entity);
    }

    [Authorize(EHubPermissions.ShopCashRegisters.Create)]
    public async Task<ShopCashRegisterDto> CreateAsync(CreateUpdateShopCashRegisterDto input)
    {
        var entity = await _manager.CreateAsync(input.Code, input.Name, input.Description, input.IsDefault, input.IsActive);
        await _registerRepository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCashRegisters.Edit)]
    public async Task<ShopCashRegisterDto> UpdateAsync(Guid id, CreateUpdateShopCashRegisterDto input)
    {
        var entity = await FindRegisterAsync(id);
        await _manager.UpdateAsync(entity, input.Code, input.Name, input.Description, input.IsDefault, input.IsActive);
        await _registerRepository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    [Authorize(EHubPermissions.ShopCashRegisters.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var entity = await FindRegisterAsync(id);
        await _manager.ValidateDeleteAsync(entity);
        await _registerRepository.DeleteAsync(entity, autoSave: true);
    }

    [Authorize(EHubPermissions.ShopCashRegisters.Default)]
    public async Task<ListResultDto<ShopCashRegisterLookupDto>> GetLookupAsync()
    {
        if (!CurrentTenant.Id.HasValue) return new ListResultDto<ShopCashRegisterLookupDto>(new List<ShopCashRegisterLookupDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _registerRepository.GetQueryableAsync();
        var entities = await AsyncExecuter.ToListAsync(query.Where(x => x.TenantId == tenantId && x.IsActive).OrderBy(x => x.Name));
        var items = ObjectMapper.Map<List<ShopCashRegister>, List<ShopCashRegisterLookupDto>>(entities);
        return new ListResultDto<ShopCashRegisterLookupDto>(items);
    }

    // ---------------------------------------------------------------------
    // Closing
    // ---------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopCashClosings.Default)]
    public async Task<ShopCashClosingDto?> GetOpenClosingAsync(Guid cashRegisterId)
    {
        var tenantId = RequireTenant();
        var closing = await _manager.FindOpenClosingAsync(cashRegisterId, tenantId);
        if (closing == null) return null;

        var summary = await _manager.ComputeSummaryAsync(closing, tenantId);
        var dto = await MapClosingToDtoAsync(closing);
        ApplyLiveSummary(dto, summary);
        await HideClosingAmountsIfNotAllowedAsync(new List<ShopCashClosingDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopCashClosings.Default)]
    public async Task<ShopCashClosingDto> GetClosingAsync(Guid closingId)
    {
        var closing = await FindClosingAsync(closingId);
        var dto = await MapClosingToDtoAsync(closing);

        if (closing.Status == ShopCashClosingStatus.Open)
        {
            var tenantId = RequireTenant();
            var summary = await _manager.ComputeSummaryAsync(closing, tenantId);
            ApplyLiveSummary(dto, summary);
        }

        await HideClosingAmountsIfNotAllowedAsync(new List<ShopCashClosingDto> { dto });
        return dto;
    }

    [Authorize(EHubPermissions.ShopCashClosings.Default)]
    public async Task<PagedResultDto<ShopCashClosingDto>> GetClosingsAsync(GetShopCashTransactionsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopCashClosingDto>(0, new List<ShopCashClosingDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _closingRepository.GetQueryableAsync();
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.CashRegisterId.HasValue, x => x.CashRegisterId == input.CashRegisterId)
            .WhereIf(input.TransactionDateFrom.HasValue, x => x.BusinessDate >= input.TransactionDateFrom!.Value)
            .WhereIf(input.TransactionDateTo.HasValue, x => x.BusinessDate <= input.TransactionDateTo!.Value);

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "BusinessDate desc, CreationTime desc" : input.Sorting!;
        var entities = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = new List<ShopCashClosingDto>();
        foreach (var entity in entities) items.Add(await MapClosingToDtoAsync(entity));
        await HideClosingAmountsIfNotAllowedAsync(items);

        return new PagedResultDto<ShopCashClosingDto>(totalCount, items);
    }

    [Authorize(EHubPermissions.ShopCashClosings.Open)]
    public async Task<ShopCashClosingDto> OpenAsync(Guid cashRegisterId, OpenShopCashRegisterDto input)
    {
        var closing = await _manager.OpenAsync(cashRegisterId, input.BusinessDate, input.OpeningCash, input.Notes);
        return await GetClosingAsync(closing.Id);
    }

    [Authorize(EHubPermissions.ShopCashClosings.Close)]
    public async Task<ShopCashClosingDto> CloseAsync(Guid closingId, CloseShopCashRegisterDto input)
    {
        var closing = await FindClosingAsync(closingId);
        await _manager.CloseAsync(closing, input.ActualClosingCash, input.Notes);
        await _closingRepository.UpdateAsync(closing, autoSave: true);
        return await GetClosingAsync(closing.Id);
    }

    [Authorize(EHubPermissions.ShopCashClosings.Cancel)]
    public async Task<ShopCashClosingDto> CancelClosingAsync(Guid closingId, CancelShopCashClosingDto input)
    {
        var closing = await FindClosingAsync(closingId);
        await _manager.CancelClosingAsync(closing, input.CancellationReason);
        await _closingRepository.UpdateAsync(closing, autoSave: true);
        return await GetClosingAsync(closing.Id);
    }

    // ---------------------------------------------------------------------
    // Manual cash movement
    // ---------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopCashTransactions.ManualMovement)]
    public async Task<ShopCashRegisterTransactionDto> CreateManualMovementAsync(CreateManualCashMovementDto input)
    {
        var transaction = await _manager.CreateManualMovementAsync(
            input.CashRegisterId, input.TransactionDate, input.Direction, input.Amount, input.ReferenceNumber, input.Description);

        var dto = await MapTransactionToDtoAsync(transaction);
        await HideTransactionAmountsIfNotAllowedAsync(new List<ShopCashRegisterTransactionDto> { dto });
        return dto;
    }

    // ---------------------------------------------------------------------
    // Transactions
    // ---------------------------------------------------------------------

    [Authorize(EHubPermissions.ShopCashTransactions.Default)]
    public async Task<PagedResultDto<ShopCashRegisterTransactionDto>> GetTransactionsAsync(GetShopCashTransactionsInput input)
    {
        if (!CurrentTenant.Id.HasValue) return new PagedResultDto<ShopCashRegisterTransactionDto>(0, new List<ShopCashRegisterTransactionDto>());
        var tenantId = CurrentTenant.Id.Value;

        var query = await _transactionRepository.GetQueryableAsync();
        var filtered = query.Where(x => x.TenantId == tenantId)
            .WhereIf(input.CashRegisterId.HasValue, x => x.CashRegisterId == input.CashRegisterId)
            .WhereIf(input.CashClosingId.HasValue, x => x.CashClosingId == input.CashClosingId)
            .WhereIf(input.TransactionType.HasValue, x => x.TransactionType == input.TransactionType)
            .WhereIf(input.Direction.HasValue, x => x.Direction == input.Direction)
            .WhereIf(input.TransactionDateFrom.HasValue, x => x.TransactionDate >= input.TransactionDateFrom!.Value)
            .WhereIf(input.TransactionDateTo.HasValue, x => x.TransactionDate <= input.TransactionDateTo!.Value)
            .WhereIf(!input.Filter.IsNullOrWhiteSpace(), x =>
                x.ReferenceNumber.Contains(input.Filter!) || (x.Description != null && x.Description.Contains(input.Filter!)));

        var totalCount = await AsyncExecuter.CountAsync(filtered);
        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "TransactionDate desc, CreationTime desc" : input.Sorting!;
        var rows = await AsyncExecuter.ToListAsync(filtered.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var runningBalanceById = await ComputeRunningBalancesAsync(tenantId, input.CashRegisterId);

        var registerIds = rows.Select(x => x.CashRegisterId).Distinct().ToList();
        var registerQuery = await _registerRepository.GetQueryableAsync();
        var registers = registerQuery.Where(x => registerIds.Contains(x.Id)).ToList().ToDictionary(x => x.Id);

        var canViewAmounts = await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCashTransactions.ViewAmounts);

        var items = rows.Select(x =>
        {
            registers.TryGetValue(x.CashRegisterId, out var register);
            return new ShopCashRegisterTransactionDto
            {
                Id = x.Id,
                CashRegisterId = x.CashRegisterId,
                CashRegisterCode = register?.Code ?? string.Empty,
                CashRegisterName = register?.Name ?? string.Empty,
                CashClosingId = x.CashClosingId,
                TransactionDate = x.TransactionDate,
                TransactionType = x.TransactionType,
                Direction = x.Direction,
                Amount = canViewAmounts ? x.Amount : null,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                ReferenceNumber = x.ReferenceNumber,
                Description = x.Description,
                RunningBalance = canViewAmounts && runningBalanceById.TryGetValue(x.Id, out var balance) ? balance : null,
                CreationTime = x.CreationTime
            };
        }).ToList();

        return new PagedResultDto<ShopCashRegisterTransactionDto>(totalCount, items);
    }

    [Authorize(EHubPermissions.ShopCashClosings.Default)]
    public async Task<ShopCashRegisterSummaryDto> GetSummaryAsync(Guid closingId)
    {
        var tenantId = RequireTenant();
        var closing = await FindClosingAsync(closingId);

        var dto = new ShopCashRegisterSummaryDto();

        if (closing.Status == ShopCashClosingStatus.Open)
        {
            var summary = await _manager.ComputeSummaryAsync(closing, tenantId);
            dto.OpeningCash = closing.OpeningCash;
            dto.CashSales = summary.CashSales;
            dto.CustomerCashPayments = summary.CustomerCashPayments;
            dto.SupplierCashPayments = summary.SupplierCashPayments;
            dto.CashExpenses = summary.CashExpenses;
            dto.CustomerRefunds = summary.CustomerRefunds;
            dto.ManualCashIn = summary.ManualCashIn;
            dto.ManualCashOut = summary.ManualCashOut;
            dto.ExpectedClosingCash = summary.ExpectedClosingCash;
            dto.ActualClosingCash = null;
            dto.DifferenceAmount = null;
        }
        else
        {
            dto.OpeningCash = closing.OpeningCash;
            dto.CashSales = closing.CashSales;
            dto.CustomerCashPayments = closing.CustomerCashPayments;
            dto.SupplierCashPayments = closing.SupplierCashPayments;
            dto.CashExpenses = closing.CashExpenses;
            dto.CustomerRefunds = closing.CustomerRefunds;
            dto.ManualCashIn = closing.ManualCashIn;
            dto.ManualCashOut = closing.ManualCashOut;
            dto.ExpectedClosingCash = closing.ExpectedClosingCash;
            dto.ActualClosingCash = closing.ActualClosingCash;
            dto.DifferenceAmount = closing.DifferenceAmount;
        }

        dto.IsShort = dto.DifferenceAmount.HasValue && dto.DifferenceAmount.Value < 0;
        dto.IsExcess = dto.DifferenceAmount.HasValue && dto.DifferenceAmount.Value > 0;

        await HideSummaryAmountsIfNotAllowedAsync(dto);
        return dto;
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private async Task<Dictionary<Guid, decimal>> ComputeRunningBalancesAsync(Guid tenantId, Guid? cashRegisterId)
    {
        var result = new Dictionary<Guid, decimal>();
        if (!cashRegisterId.HasValue) return result;

        var query = await _transactionRepository.GetQueryableAsync();
        var all = await AsyncExecuter.ToListAsync(query
            .Where(x => x.TenantId == tenantId && x.CashRegisterId == cashRegisterId.Value)
            .OrderBy(x => x.TransactionDate).ThenBy(x => x.CreationTime));

        decimal running = 0;
        foreach (var transaction in all)
        {
            running += transaction.Direction == ShopCashDirection.In ? transaction.Amount : -transaction.Amount;
            result[transaction.Id] = Math.Round(running, 2, MidpointRounding.AwayFromZero);
        }

        return result;
    }

    private async Task<ShopCashClosingDto> MapClosingToDtoAsync(ShopCashClosing entity)
    {
        var register = await _registerRepository.GetAsync(entity.CashRegisterId);
        return new ShopCashClosingDto
        {
            Id = entity.Id,
            CashRegisterId = entity.CashRegisterId,
            CashRegisterCode = register.Code,
            CashRegisterName = register.Name,
            BusinessDate = entity.BusinessDate,
            Status = entity.Status,
            OpeningCash = entity.OpeningCash,
            CashSales = entity.CashSales,
            CustomerCashPayments = entity.CustomerCashPayments,
            SupplierCashPayments = entity.SupplierCashPayments,
            CashExpenses = entity.CashExpenses,
            CustomerRefunds = entity.CustomerRefunds,
            ManualCashIn = entity.ManualCashIn,
            ManualCashOut = entity.ManualCashOut,
            ExpectedClosingCash = entity.ExpectedClosingCash,
            ActualClosingCash = entity.ActualClosingCash,
            DifferenceAmount = entity.DifferenceAmount,
            Notes = entity.Notes,
            CancellationReason = entity.CancellationReason,
            OpenedByUserId = entity.OpenedByUserId,
            OpenedDate = entity.OpenedDate,
            ClosedByUserId = entity.ClosedByUserId,
            ClosedDate = entity.ClosedDate,
            CreationTime = entity.CreationTime
        };
    }

    private static void ApplyLiveSummary(
        ShopCashClosingDto dto,
        (decimal CashSales, decimal CustomerCashPayments, decimal SupplierCashPayments, decimal CashExpenses,
            decimal CustomerRefunds, decimal ManualCashIn, decimal ManualCashOut, decimal ExpectedClosingCash) summary)
    {
        dto.CashSales = summary.CashSales;
        dto.CustomerCashPayments = summary.CustomerCashPayments;
        dto.SupplierCashPayments = summary.SupplierCashPayments;
        dto.CashExpenses = summary.CashExpenses;
        dto.CustomerRefunds = summary.CustomerRefunds;
        dto.ManualCashIn = summary.ManualCashIn;
        dto.ManualCashOut = summary.ManualCashOut;
        dto.ExpectedClosingCash = summary.ExpectedClosingCash;
    }

    private async Task<ShopCashRegisterTransactionDto> MapTransactionToDtoAsync(ShopCashRegisterTransaction entity)
    {
        var register = await _registerRepository.GetAsync(entity.CashRegisterId);
        return new ShopCashRegisterTransactionDto
        {
            Id = entity.Id,
            CashRegisterId = entity.CashRegisterId,
            CashRegisterCode = register.Code,
            CashRegisterName = register.Name,
            CashClosingId = entity.CashClosingId,
            TransactionDate = entity.TransactionDate,
            TransactionType = entity.TransactionType,
            Direction = entity.Direction,
            Amount = entity.Amount,
            ReferenceType = entity.ReferenceType,
            ReferenceId = entity.ReferenceId,
            ReferenceNumber = entity.ReferenceNumber,
            Description = entity.Description,
            CreationTime = entity.CreationTime
        };
    }

    private async Task HideClosingAmountsIfNotAllowedAsync(List<ShopCashClosingDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCashClosings.ViewAmounts)) return;
        foreach (var dto in items)
        {
            dto.OpeningCash = null;
            dto.CashSales = null;
            dto.CustomerCashPayments = null;
            dto.SupplierCashPayments = null;
            dto.CashExpenses = null;
            dto.CustomerRefunds = null;
            dto.ManualCashIn = null;
            dto.ManualCashOut = null;
            dto.ExpectedClosingCash = null;
            dto.ActualClosingCash = null;
            dto.DifferenceAmount = null;
        }
    }

    private async Task HideTransactionAmountsIfNotAllowedAsync(List<ShopCashRegisterTransactionDto> items)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCashTransactions.ViewAmounts)) return;
        foreach (var dto in items)
        {
            dto.Amount = null;
            dto.RunningBalance = null;
        }
    }

    private async Task HideSummaryAmountsIfNotAllowedAsync(ShopCashRegisterSummaryDto dto)
    {
        if (await AuthorizationService.IsGrantedAsync(EHubPermissions.ShopCashClosings.ViewAmounts)) return;
        dto.OpeningCash = null;
        dto.CashSales = null;
        dto.CustomerCashPayments = null;
        dto.SupplierCashPayments = null;
        dto.CashExpenses = null;
        dto.CustomerRefunds = null;
        dto.ManualCashIn = null;
        dto.ManualCashOut = null;
        dto.ExpectedClosingCash = null;
        dto.ActualClosingCash = null;
        dto.DifferenceAmount = null;
    }

    private Guid RequireTenant() => CurrentTenant.Id ?? throw new BusinessException("ShopManagement:TenantRequired");

    private async Task<ShopCashRegister> FindRegisterAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _registerRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:CashRegisterNotFound");
    }

    private async Task<ShopCashClosing> FindClosingAsync(Guid id)
    {
        var tenantId = RequireTenant();
        var query = await _closingRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TenantId == tenantId))
            ?? throw new BusinessException("ShopManagement:CashClosingNotFound");
    }
}
