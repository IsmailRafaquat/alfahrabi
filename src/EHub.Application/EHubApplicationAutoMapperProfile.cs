using AutoMapper;
using EHub.ShopManagement.Settings;
using EHub.ShopManagement.ProductCategories;
using EHub.ShopManagement.Units;
using EHub.ShopManagement.Products;
using EHub.ShopManagement.Suppliers;
using EHub.ShopManagement.PurchaseOrders;
using EHub.ShopManagement.GoodsReceipts;
using EHub.ShopManagement.StockTransactions;
using EHub.ShopManagement.SupplierPayments;
using EHub.ShopManagement.Customers;
using EHub.ShopManagement.ExpenseCategories;
using EHub.Expenses.ExpenseCategories;
using EHub.Expenses.ExpenseEntries;
using EHub.Expenses.StaffSalaryPayments;
using EHub.FeeModule.FeeHeads;
using EHub.FeeModule.FeeStructureItems;
using EHub.FeeModule.FeeStructures;
using EHub.FeeModule.LateFeePolicies;
using EHub.FeeModule.StudentFeeDiscounts;
using EHub.FeeModule.StudentFeeProfiles;
using EHub.FeeModule.StudentMonthlyFeeLines;
using EHub.FeeModule.StudentMonthlyFees;
using EHub.FileAttachments;
using EHub.Reports.ExpenseReport;
using EHub.Reports.SalaryReport;
using EHub.StaffAttendances;
using EHub.StaffDocuments;
using EHub.Staffs;
using EHub.StudentAttendances;
using EHub.StudentDocuments;
using EHub.Students;
using EHub.Subjects;

namespace EHub;

public class EHubApplicationAutoMapperProfile : Profile
{
    public EHubApplicationAutoMapperProfile()
    {
        CreateMap<ShopSetting, ShopSettingDto>();
        CreateMap<CreateUpdateShopSettingDto, ShopSetting>();
        CreateMap<ShopProductCategory, ShopProductCategoryDto>();
        CreateMap<ShopProductCategory, ShopProductCategoryLookupDto>();
        CreateMap<ShopUnit, ShopUnitDto>();
        CreateMap<ShopUnit, ShopUnitLookupDto>();
        CreateMap<ShopProduct, ShopProductDto>()
            .ForMember(x => x.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(x => x.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : string.Empty))
            .ForMember(x => x.UnitShortName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.ShortName : string.Empty))
            .ForMember(x => x.UnitAllowDecimal, opt => opt.MapFrom(src => src.Unit != null && src.Unit.AllowDecimal));
        CreateMap<ShopCustomer, ShopCustomerDto>();
        CreateMap<ShopCustomer, ShopCustomerLookupDto>();
        CreateMap<ShopSupplier, ShopSupplierDto>();
        CreateMap<ShopSupplier, ShopSupplierLookupDto>()
            .ForMember(x => x.DisplayName, opt => opt.MapFrom(src => src.Code + " - " + src.Name));
        CreateMap<ShopExpenseCategory, ShopExpenseCategoryDto>();
        CreateMap<ShopExpenseCategory, ShopExpenseCategoryLookupDto>();
        CreateMap<ShopPurchaseOrder, ShopPurchaseOrderDto>()
            .ForMember(x => x.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : string.Empty))
            .ForMember(x => x.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty));
        CreateMap<ShopPurchaseOrderItem, ShopPurchaseOrderItemDto>()
            .ForMember(x => x.ProductName, opt => opt.MapFrom(src => src.ProductNameSnapshot))
            .ForMember(x => x.ProductCode, opt => opt.MapFrom(src => src.ProductCodeSnapshot))
            .ForMember(x => x.UnitName, opt => opt.MapFrom(src => src.UnitNameSnapshot))
            .ForMember(x => x.UnitShortName, opt => opt.MapFrom(src => src.UnitShortNameSnapshot));
        CreateMap<ShopGoodsReceipt, ShopGoodsReceiptDto>()
            .ForMember(x => x.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : string.Empty))
            .ForMember(x => x.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForMember(x => x.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.PurchaseOrderNumber : string.Empty));
        CreateMap<ShopGoodsReceiptItem, ShopGoodsReceiptItemDto>()
            .ForMember(x => x.ProductName, opt => opt.MapFrom(src => src.ProductNameSnapshot))
            .ForMember(x => x.ProductCode, opt => opt.MapFrom(src => src.ProductCodeSnapshot))
            .ForMember(x => x.UnitName, opt => opt.MapFrom(src => src.UnitNameSnapshot))
            .ForMember(x => x.UnitShortName, opt => opt.MapFrom(src => src.UnitShortNameSnapshot))
            .ForMember(x => x.OrderedQuantity, opt => opt.MapFrom(src => src.OrderedQuantitySnapshot))
            .ForMember(x => x.RemainingQuantity, opt => opt.MapFrom(src => src.OrderedQuantitySnapshot - src.PreviouslyReceivedQuantity));
        CreateMap<ShopStockTransaction, ShopStockTransactionDto>()
            .ForMember(x => x.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            .ForMember(x => x.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty));
        CreateMap<ShopSupplierPayment, ShopSupplierPaymentDto>()
            .ForMember(x => x.SupplierCode, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Code : string.Empty))
            .ForMember(x => x.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            .ForMember(x => x.AllocatedAmount, opt => opt.Ignore())
            .ForMember(x => x.UnallocatedAmount, opt => opt.Ignore());
        CreateMap<ShopSupplierPaymentAllocation, ShopSupplierPaymentAllocationDto>()
            .ForMember(x => x.GoodsReceiptNumber, opt => opt.MapFrom(src => src.GoodsReceipt != null ? src.GoodsReceipt.GoodsReceiptNumber : string.Empty))
            .ForMember(x => x.SupplierInvoiceNumber, opt => opt.MapFrom(src => src.GoodsReceipt != null ? src.GoodsReceipt.SupplierInvoiceNumber : null))
            .ForMember(x => x.ReceiptDate, opt => opt.MapFrom(src => src.GoodsReceipt != null ? src.GoodsReceipt.ReceiptDate : default))
            .ForMember(x => x.GrandTotal, opt => opt.MapFrom(src => src.GoodsReceipt != null ? (decimal?)src.GoodsReceipt.GrandTotal : null));
        CreateMap<ShopProduct, ShopProductLookupDto>()
            .ForMember(x => x.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(x => x.UnitName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : string.Empty))
            .ForMember(x => x.UnitShortName, opt => opt.MapFrom(src => src.Unit != null ? src.Unit.ShortName : string.Empty))
            .ForMember(x => x.UnitAllowDecimal, opt => opt.MapFrom(src => src.Unit != null && src.Unit.AllowDecimal));
        CreateMap<Student, StudentDto>()
            .ForMember(x => x.StudentDocument, opt => opt.MapFrom(src => src.StudentDocuments));
        CreateMap<Staff, StaffDto>()
            .ForMember(x => x.StaffDocuments, opt => opt.MapFrom(src => src.StaffDocuments));

        CreateMap<Subject, SubjectDto>();

        CreateMap<StudentDocument, StudentDocumentDto>()
            .ForMember(x => x.FileAttachment, opt => opt.MapFrom(src => src.FileAttachment));
        CreateMap<StaffDocument, StaffDocumentDto>()
            .ForMember(x => x.FileAttachments, opt => opt.MapFrom(src => src.FileAttachments));

        CreateMap<FileAttachment, FileAttachmentDto>();

        CreateMap<StudentAttendance, StudentAttendanceDto>()
            .ForMember(x => x.StudentName, opt => opt.MapFrom(src => src.Students.FirstName + ' ' + src.Students.LastName))
            .ForMember(x => x.AdmissionNo, opt => opt.MapFrom(src => src.Students.AdmissionNo));

        CreateMap<StaffAttendance, StaffAttendanceDto>()
       .ForMember(x => x.StaffName, opt => opt.MapFrom(src => src.Staff.FirstName + ' ' + src.Staff.LastName))
       .ForMember(x => x.EmployeeCode, opt => opt.MapFrom(src => src.Staff.EmployeeCode));

        CreateMap<FeeHead, FeeHeadDto>();
        CreateMap<FeeStructure, FeeStructureDto>();
        CreateMap<FeeStructureItem, FeeStructureItemDto>();
        CreateMap<StudentFeeProfile, StudentFeeProfileDto>();

        CreateMap<StudentFeeDiscount, StudentFeeDiscountDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src =>
                src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : string.Empty))
            .ForMember(dest => dest.FeeHeadName, opt => opt.MapFrom(src =>
                src.FeeHead != null ? src.FeeHead.Name : "Total Fee"))
            .ForMember(dest => dest.ApprovedByStaffName, opt => opt.MapFrom(src =>
                src.ApprovedByStaff != null ? $"{src.ApprovedByStaff.FirstName} {src.ApprovedByStaff.LastName}" : null));

        CreateMap<LateFeePolicy, LateFeePolicyDto>()
           .ForMember(dest => dest.IsGlobal, opt => opt.MapFrom(src =>
               !src.GradeLevel.HasValue &&
               !src.Section.HasValue &&
               !src.Shift.HasValue &&
               !src.Term.HasValue));

        CreateMap<StudentMonthlyFee, StudentMonthlyFeeDto>()
            .ForMember(d => d.StudentName, opt => opt.MapFrom(src =>
                src.Student != null ? $"{src.Student.FirstName} {src.Student.LastName}" : null))
            .ForMember(d => d.AdmissionNo, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.AdmissionNo : null))
            .ForMember(d => d.GradeLevel, opt => opt.MapFrom(src =>
                src.Student != null ? src.Student.GradeLevel : (GradeLevel?)null));

        CreateMap<StudentMonthlyFeeLine, StudentMonthlyFeeLineDto>()
            .ForMember(d => d.FeeHeadName, opt => opt.MapFrom(src => src.FeeHead != null ? src.FeeHead.Name : null));

        CreateMap<ExpenseCategory, ExpenseCategoryDto>();
        CreateMap<ExpenseEntry, ExpenseEntryDto>();
        CreateMap<StaffSalaryPayment, StaffSalaryPaymentDto>();
        CreateMap<ExpenseReport, ExpenseReportDto>();
        CreateMap<StaffSalaryReport, StaffSalaryReportDto>();
    }
}
