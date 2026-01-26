using EHub.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace EHub.FeeModule.StudentFeeDiscounts;

public class EfCoreStudentFeeDiscountRepository
    : EfCoreRepository<EHubDbContext, StudentFeeDiscount, Guid>,
      IStudentFeeDiscountRepository
{
    public EfCoreStudentFeeDiscountRepository(IDbContextProvider<EHubDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<StudentFeeDiscount?> GetByIdAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        Guid? feeHeadId,
        DiscountType? discountType,
        bool? isActive,
        bool? isApproved)
    {
        var data = await GetFiltersAsync(filter, studentId, feeHeadId, discountType, isActive, isApproved);
        return await data.LongCountAsync();
    }

    public async Task<List<StudentFeeDiscount>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        Guid? feeHeadId,
        DiscountType? discountType,
        bool? isActive,
        bool? isApproved)
    {
        var data = await GetFiltersAsync(filter, studentId, feeHeadId, discountType, isActive, isApproved);
        return await data
            .OrderBy(sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync();
    }

    private async Task<IQueryable<StudentFeeDiscount>> GetFiltersAsync(
        string? filter,
        Guid? studentId,
        Guid? feeHeadId,
        DiscountType? discountType,
        bool? isActive,
        bool? isApproved)
    {
        var queryable = await GetQueryableAsync();
        var query = queryable
            .Include(x => x.ApprovedByStaff)
            .Include(x => x.Student)
            .Include(x => x.FeeHead).AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(filter),
                x => x.Reason.Contains(filter!)
                  || x.StudentId.ToString().Contains(filter!))
            .WhereIf(studentId.HasValue, x => x.StudentId == studentId)
            .WhereIf(feeHeadId.HasValue, x => x.FeeHeadId == feeHeadId)
            .WhereIf(discountType.HasValue, x => x.DiscountType == discountType)
            .WhereIf(isActive.HasValue, x => x.IsActive == isActive)
            .WhereIf(isApproved.HasValue && isApproved.Value, x => x.ApprovedByStaffId != null)
            .WhereIf(isApproved.HasValue && !isApproved.Value, x => x.ApprovedByStaffId == null);

        return query;
    }

    public async Task<List<StudentFeeDiscount>> GetActiveDiscountsByStudentAsync(
        Guid studentId,
        Guid? feeHeadId = null)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.StudentId == studentId && x.IsActive)
            .WhereIf(feeHeadId.HasValue, x => x.FeeHeadId == feeHeadId)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingDiscountAsync(
        Guid studentId,
        Guid? feeHeadId,
        int? startMonth,
        int? endMonth,
        Guid? exceptId = null)
    {
        var dbSet = await GetDbSetAsync();

        var query = dbSet
            .Where(x => x.StudentId == studentId && x.IsActive)
            .Where(x => x.FeeHeadId == feeHeadId) // Same fee head (or both null)
            .WhereIf(exceptId.HasValue, x => x.Id != exceptId!.Value);

        // Check for month overlap
        var overlapping = await query
            .Where(x =>
                // Both have no month range
                (!x.StartMonth.HasValue && !x.EndMonth.HasValue && !startMonth.HasValue && !endMonth.HasValue) ||
                // Current has no range (applies all year)
                (!x.StartMonth.HasValue && !x.EndMonth.HasValue) ||
                // New has no range (applies all year)
                (!startMonth.HasValue && !endMonth.HasValue) ||
                // Both have ranges - check overlap
                (x.StartMonth.HasValue && x.EndMonth.HasValue &&
                 startMonth.HasValue && endMonth.HasValue &&
                 !(x.EndMonth.Value < startMonth.Value || x.StartMonth.Value > endMonth.Value))
            )
            .AnyAsync();

        return overlapping;
    }

    public async Task<StudentFeeDiscount?> GetApplicableDiscountAsync(
       Guid studentId,
       Guid? feeHeadId,
       DateTime month)
    {
        var dbSet = await GetDbSetAsync();

        // Convert DateTime to YYYYMM format for comparison
        // Assuming StartMonth and EndMonth are stored as integers in YYYYMM format
        // Example: January 2026 = 202601
        int monthValue = month.Year * 100 + month.Month;

        // Priority:
        // 1. Student-specific + FeeHead-specific + Active + Approved + Within date range
        // 2. Student-specific + All FeeHeads (FeeHeadId is null) + Active + Approved + Within date range

        var query = dbSet
            .Where(d => d.StudentId == studentId)
            .Where(d => d.IsActive)
            .Where(d => (!d.StartMonth.HasValue || d.StartMonth.Value <= monthValue))
            .Where(d => (!d.EndMonth.HasValue || d.EndMonth.Value >= monthValue));

        // Try to find fee-head specific discount first
        if (feeHeadId.HasValue)
        {
            var specificDiscount = await query
                .Where(d => d.FeeHeadId == feeHeadId.Value)
                .OrderByDescending(d => d.CreationTime)
                .FirstOrDefaultAsync();

            if (specificDiscount != null)
                return specificDiscount;
        }

        // Fallback to general discount (all fee heads)
        return await query
            .Where(d => d.FeeHeadId == null)
            .OrderByDescending(d => d.CreationTime)
            .FirstOrDefaultAsync();
    }
}