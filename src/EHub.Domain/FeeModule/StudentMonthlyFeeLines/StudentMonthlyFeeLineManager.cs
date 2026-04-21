using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructureItems;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.FeeModule.StudentMonthlyFees;
using System;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Guids;

public class StudentMonthlyFeeLineManager : DomainService
{
    private readonly IStudentMonthlyFeeLineRepository _repo;
    private readonly IRepository<FeeStructureItem, Guid> _feeStructureItemRepo;
    private readonly IRepository<FeeHead, Guid> _feeHeadRepo;
    private readonly IRepository<StudentMonthlyFee, Guid> _studentMonthlyFeeRepo;
    public StudentMonthlyFeeLineManager(
        IStudentMonthlyFeeLineRepository repo,
        IRepository<FeeStructureItem, Guid> feeStructureItemRepo,
        IRepository<FeeHead, Guid> feeHeadRepo,
        IRepository<StudentMonthlyFee, Guid> studentMonthlyFeeRepo)
    {
        _repo = repo;
        _feeStructureItemRepo = feeStructureItemRepo;
        _feeHeadRepo = feeHeadRepo;
        _studentMonthlyFeeRepo = studentMonthlyFeeRepo;
    }
    public async Task<StudentMonthlyFeeLine> CreateAsync(
        Guid studentMonthlyFeeId,
        Guid feeHeadId,
        decimal expectedAmount,
        decimal discountAmount,
        decimal adjustmentAmount,
        decimal lateFeeAmount,
        decimal paidAmount)
    {
        if (studentMonthlyFeeId == Guid.Empty) throw new UserFriendlyException("StudentMonthlyFeeId is required.");
        if (feeHeadId == Guid.Empty) throw new UserFriendlyException("FeeHeadId is required.");
        var exists = await _repo.AnyAsync(x =>
            x.StudentMonthlyFeeId == studentMonthlyFeeId &&
            x.FeeHeadId == feeHeadId);
        if (exists)
            throw new UserFriendlyException("This fee head already exists for this monthly fee.");

        var oneTimeAlreadyUsed = await IsOneTimeFeeAlreadyUsedAsync(studentMonthlyFeeId, feeHeadId);
        if (oneTimeAlreadyUsed)
            throw new UserFriendlyException("This fee head can only be added once in the session.");

        var netAmount = expectedAmount - discountAmount + adjustmentAmount + lateFeeAmount;

        if (paidAmount > netAmount)
            throw new UserFriendlyException($"Paid amount cannot be greater than net amount ({netAmount:0.##}).");

        return new StudentMonthlyFeeLine(
            GuidGenerator.Create(),
            studentMonthlyFeeId,
            feeHeadId,
            expectedAmount,
            discountAmount,
            adjustmentAmount,
            lateFeeAmount,
            paidAmount);
    }
    /// <summary>
    /// Generates lines for a monthly fee based on FeeStructureItems (FeeHead + MonthlyAmount).
    /// skipIfExists=true => don't create if a line already exists for (StudentMonthlyFeeId, FeeHeadId).
    /// </summary>
    public async Task<(int created, int skipped)> GenerateLinesFromFeeStructureAsync(
        Guid studentMonthlyFeeId,
        Guid feeStructureId,
        bool skipIfExists)
    {
        if (studentMonthlyFeeId == Guid.Empty) throw new UserFriendlyException("StudentMonthlyFeeId is required.");
        if (feeStructureId == Guid.Empty) throw new UserFriendlyException("FeeStructureId is required.");
        // 1) Read fee structure items via IRepository queryable
        var fsQ = await _feeStructureItemRepo.GetQueryableAsync();
        var fsItems = fsQ
            .Where(x => x.FeeStructureId == feeStructureId)
            .OrderBy(x => x.CreationTime)
            .ToList();
        if (fsItems.Count == 0)
            return (0, 0);
        // 2) Pre-load existing lines for this monthly fee
        var existing = await _repo.GetByMonthlyFeeAsync(studentMonthlyFeeId);
        var existingHeadIds = existing.Select(x => x.FeeHeadId).ToHashSet();
        var created = 0;
        var skipped = 0;
        foreach (var item in fsItems)
        {
            if (existingHeadIds.Contains(item.FeeHeadId))
            {
                // either skip only when skipIfExists, or treat as skip always (your current behavior)
                skipped++;
                continue;
            }

            var oneTimeAlreadyUsed = await IsOneTimeFeeAlreadyUsedAsync(studentMonthlyFeeId, item.FeeHeadId);
            if (oneTimeAlreadyUsed)
            {
                skipped++;
                continue;
            }

            var line = new StudentMonthlyFeeLine(
                GuidGenerator.Create(),
                studentMonthlyFeeId,
                item.FeeHeadId,
                expectedAmount: item.MonthlyAmount,
                discountAmount: 0,
                adjustmentAmount: 0,
                lateFeeAmount: 0,
                paidAmount: 0
            );
            await _repo.InsertAsync(line, autoSave: false);
            existingHeadIds.Add(item.FeeHeadId); // important for in-loop duplicate protection
            created++;
        }
        return (created, skipped);
    }

    private async Task<bool> IsOneTimeFeeAlreadyUsedAsync(Guid studentMonthlyFeeId, Guid feeHeadId)
    {
        var feeHead = await _feeHeadRepo.GetAsync(feeHeadId);
        if (feeHead.ChargeType != FeeHeadChargeType.OneTimePerSession)
            return false;

        var currentMonthlyFee = await _studentMonthlyFeeRepo.GetAsync(studentMonthlyFeeId);

        var startOfYear = new DateTime(currentMonthlyFee.Month.Year, 1, 1);
        var endOfYear = new DateTime(currentMonthlyFee.Month.Year, 12, 1);

        var monthlyFeesQuery = await _studentMonthlyFeeRepo.GetQueryableAsync();
        var lineQuery = await _repo.GetQueryableAsync();

        var exists = (
            from line in lineQuery
            join monthlyFee in monthlyFeesQuery on line.StudentMonthlyFeeId equals monthlyFee.Id
            where monthlyFee.StudentId == currentMonthlyFee.StudentId
                  && monthlyFee.Month >= startOfYear
                  && monthlyFee.Month <= endOfYear
                  && line.FeeHeadId == feeHeadId
            select line.Id
        ).Any();

        return exists;
    }
}
