using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace EHub.FeeModule.StudentFeeDiscounts;

public interface IStudentFeeDiscountRepository : IRepository<StudentFeeDiscount, Guid>
{
    Task<StudentFeeDiscount?> GetByIdAsync(Guid id);

    Task<long> GetCountAsync(
        string? filter,
        Guid? studentId,
        Guid? feeHeadId,
        DiscountType? discountType,
        bool? isActive,
        bool? isApproved
    );

    Task<List<StudentFeeDiscount>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string sorting,
        string? filter,
        Guid? studentId,
        Guid? feeHeadId,
        DiscountType? discountType,
        bool? isActive,
        bool? isApproved
    );

    Task<List<StudentFeeDiscount>> GetActiveDiscountsByStudentAsync(
        Guid studentId,
        Guid? feeHeadId = null);

    Task<bool> HasOverlappingDiscountAsync(
        Guid studentId,
        Guid? feeHeadId,
        int? startMonth,
        int? endMonth,
        Guid? exceptId = null);

    Task<StudentFeeDiscount?> GetApplicableDiscountAsync(
       Guid studentId,
       Guid? feeHeadId,
       DateTime month);
}
