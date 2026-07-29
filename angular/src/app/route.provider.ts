import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
    {
      name: '::SchoolManagement', iconClass: 'fas fa-school', order: 1,
      layout: eLayoutType.application, requiredPolicy: 'EHub.SchoolManagement',
    },
    {
      name: '::ShopManagement', iconClass: 'fas fa-store', order: 2,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement',
    },
    {
      path: '/shop-management/dashboard', name: '::ShopDashboard', parentName: '::ShopManagement',
      iconClass: 'fas fa-tachometer-alt', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.Dashboard',
    },
    {
      path: '/shop-management/settings', name: '::ShopSettings', parentName: '::ShopManagement',
      iconClass: 'fas fa-cog', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.Settings',
    },
    {
      name: '::Products', parentName: '::ShopManagement', iconClass: 'fas fa-boxes', order: 3,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.ProductCategories',
    },
    {
      path: '/shop-management/products', name: '::ProductList', parentName: '::Products',
      iconClass: 'fas fa-cubes', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.Products',
    },
    {
      path: '/shop-management/product-categories', name: '::ProductCategories', parentName: '::Products',
      iconClass: 'fas fa-tags', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.ProductCategories',
    },
    {
      path: '/shop-management/units', name: '::Units', parentName: '::Products', iconClass: 'fas fa-balance-scale',
      order: 3, layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Units',
    },
    {
      path: '/shop-management/suppliers', name: '::Suppliers', parentName: '::ShopManagement', iconClass: 'fas fa-truck',
      order: 3, layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Suppliers',
    },
    {
      path: '/shop-management/customers', name: '::Customers', parentName: '::ShopManagement', iconClass: 'fas fa-users',
      order: 4, layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Customers',
    },
    {
      name: '::Sales', parentName: '::ShopManagement', iconClass: 'fas fa-cash-register', order: 5,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Sales',
    },
    {
      path: '/shop-management/sales', name: '::SalesPOS', parentName: '::Sales',
      iconClass: 'fas fa-receipt', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.Sales',
    },
    {
      path: '/shop-management/customer-payments', name: '::CustomerPayments', parentName: '::Sales',
      iconClass: 'fas fa-hand-holding-usd', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.CustomerPayments',
    },
    {
      path: '/shop-management/customer-ledger', name: '::CustomerLedger', parentName: '::Sales',
      iconClass: 'fas fa-book', order: 3, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.CustomerLedger',
    },
    {
      path: '/shop-management/sale-returns', name: '::SaleReturns', parentName: '::Sales',
      iconClass: 'fas fa-undo-alt', order: 4, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.SaleReturns',
    },
    {
      name: '::Purchases', parentName: '::ShopManagement', iconClass: 'fas fa-file-invoice', order: 6,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.PurchaseOrders',
    },
    {
      path: '/shop-management/purchase-orders', name: '::PurchaseOrders', parentName: '::Purchases',
      iconClass: 'fas fa-file-invoice-dollar', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.PurchaseOrders',
    },
    {
      path: '/shop-management/goods-receipts', name: '::GoodsReceipts', parentName: '::Purchases',
      iconClass: 'fas fa-dolly', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.GoodsReceipts',
    },
    {
      path: '/shop-management/supplier-payments', name: '::SupplierPayments', parentName: '::Purchases',
      iconClass: 'fas fa-money-check-alt', order: 3, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.SupplierPayments',
    },
    {
      path: '/shop-management/purchase-returns', name: '::PurchaseReturns', parentName: '::Purchases',
      iconClass: 'fas fa-undo-alt', order: 4, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.PurchaseReturns',
    },
    {
      path: '/shop-management/supplier-ledger', name: '::SupplierLedger', parentName: '::Purchases',
      iconClass: 'fas fa-book', order: 5, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.SupplierLedger',
    },
    {
      name: '::Inventory', parentName: '::ShopManagement', iconClass: 'fas fa-warehouse', order: 7,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.StockTransactions',
    },
    {
      path: '/shop-management/stock-transactions', name: '::StockTransactions', parentName: '::Inventory',
      iconClass: 'fas fa-exchange-alt', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.StockTransactions',
    },
    {
      path: '/shop-management/stock-adjustments', name: '::StockAdjustments', parentName: '::Inventory',
      iconClass: 'fas fa-balance-scale', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.StockAdjustments',
    },
    {
      path: '/shop-management/stock-counts', name: '::StockCounts', parentName: '::Inventory',
      iconClass: 'fas fa-clipboard-check', order: 3, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.StockCounts',
    },
    {
      path: '/shop-management/product-batches', name: '::ProductBatches', parentName: '::Inventory',
      iconClass: 'fas fa-boxes', order: 4, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.ProductBatches.View',
    },
    {
      name: '::ExpensesMenu', parentName: '::ShopManagement', iconClass: 'fas fa-file-invoice-dollar', order: 8,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Expenses',
    },
    {
      path: '/shop-management/expense-categories', name: '::ExpenseCategories', parentName: '::ExpensesMenu',
      iconClass: 'fas fa-tags', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.ExpenseCategories',
    },
    {
      path: '/shop-management/expenses', name: '::Expenses', parentName: '::ExpensesMenu',
      iconClass: 'fas fa-receipt', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.Expenses',
    },
    {
      name: '::CashManagement', parentName: '::ShopManagement', iconClass: 'fas fa-cash-register', order: 9,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.CashRegisters',
    },
    {
      path: '/shop-management/cash-register', name: '::CashRegister', parentName: '::CashManagement',
      iconClass: 'fas fa-cash-register', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.CashRegisters',
    },
    {
      path: '/shop-management/cash-register/transactions', name: '::CashTransactions', parentName: '::CashManagement',
      iconClass: 'fas fa-exchange-alt', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.CashTransactions',
    },
    {
      path: '/shop-management/cash-register/closings', name: '::DailyClosings', parentName: '::CashManagement',
      iconClass: 'fas fa-book', order: 3, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.CashClosings',
    },
    {
      name: '::BankManagement', parentName: '::ShopManagement', iconClass: 'fas fa-university', order: 10,
      layout: eLayoutType.application, requiredPolicy: 'ShopManagement.BankAccounts',
    },
    {
      path: '/shop-management/bank-accounts', name: '::BankAccounts', parentName: '::BankManagement',
      iconClass: 'fas fa-university', order: 1, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.BankAccounts',
    },
    {
      path: '/shop-management/bank-transactions', name: '::BankTransactions', parentName: '::BankManagement',
      iconClass: 'fas fa-exchange-alt', order: 2, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.BankTransactions',
    },
    {
      path: '/shop-management/bank-transfers', name: '::BankTransfers', parentName: '::BankManagement',
      iconClass: 'fas fa-random', order: 3, layout: eLayoutType.application,
      requiredPolicy: 'ShopManagement.BankTransfers',
    },
    {
      path: '/shop-management/reports', name: '::ShopReports', parentName: '::ShopManagement', iconClass: 'fas fa-chart-pie',
      order: 11, layout: eLayoutType.application, requiredPolicy: 'ShopManagement.Reports',
    },
    // Home / Dashboard
    {
      path: '/',
      name: '::Menu:Home',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-home',
      order: 1,
      layout: eLayoutType.application,
    },
    {
      path: '/dashboards',
      name: '::Menu:Dashboard',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-chart-line',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Dashboard',
    },
    {
      path: '/alfarabi',
      name: '::Menu:Alfarabi',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-school',
      order: 2,
      layout: eLayoutType.application,
      // requiredPolicy: 'EHub.Dashboard',
    },

    {
      name: '::Menu:Students',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-user-graduate',
      order: 10,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StudentMenu',
    },
    {
      path: '/students',
      name: '::Menu:StudentsList',
      parentName: '::Menu:Students',
      iconClass: 'fas fa-list',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StudentMenu.StudentList',
    },
    {
      path: '/student-attendance',
      name: '::Menu:StudentAttendance',
      parentName: '::Menu:Students',
      iconClass: 'fas fa-clipboard-check',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StudentMenu.StudentAttendance',
    },
    {
      path: '/student-attendance-insights',
      name: '::Menu:StudentAttendanceInsights',
      parentName: '::Menu:Students',
      iconClass: 'fas fa-chart-bar',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StudentMenu.StudentAttendanceInsights',
    },

    // -------------------------
    // Staff (Parent)
    // -------------------------
    {
      name: '::Menu:Staff',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-user-tie',
      order: 20,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StaffMenu',
    },
    {
      path: '/staffs',
      name: '::Menu:StaffList',
      parentName: '::Menu:Staff',
      iconClass: 'fas fa-list',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StaffMenu.StaffList',
    },
    {
      path: '/staff-attendances',
      name: '::Menu:StaffAttendance',
      parentName: '::Menu:Staff',
      iconClass: 'fas fa-clipboard-check',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StaffMenu.StaffAttendance',
    },
    {
      path: '/staff-attendance-insights',
      name: '::Menu:StaffAttendanceInsights',
      parentName: '::Menu:Staff',
      iconClass: 'fas fa-chart-bar',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.StaffMenu.StaffAttendanceInsights',
    },
    // Example future routes:
    // {
    //   path: '/staff-attendance',
    //   name: '::Menu:StaffAttendance',
    //   parentName: '::Menu:Staff',
    //   iconClass: 'fas fa-clipboard-check',
    //   order: 2,
    //   layout: eLayoutType.application,
    // },

    // -------------------------
    // Academics (optional parent for subjects)
    // -------------------------
    // {
    //   name: '::Menu:Academics',
    //   iconClass: 'fas fa-book',
    //   order: 30,
    //   layout: eLayoutType.application,
    // },
    // {
    //   path: '/subjects',
    //   name: '::Menu:Subjects',
    //   parentName: '::Menu:Academics',
    //   iconClass: 'fas fa-book-open',
    //   order: 1,
    //   layout: eLayoutType.application,
    // },
    {
      name: '::Menu:Fees',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-money-bill-wave',
      order: 40,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu',
    },
    {
      path: '/fee-heads',
      name: '::Menu:FeeHeads',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-list',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.FeeHead',
    },
    {
      path: '/fee-structures',
      name: '::Menu:FeeStructures',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-sitemap',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.FeeStructure',
    },
    {
      path: '/fee-structure-items',
      name: '::Menu:FeeStructureItems',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-layer-group',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.FeeStructureItem',
    },
    {
      path: '/student-fee-profiles',
      name: '::Menu:StudentFeeProfiles',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-layer-group',
      order: 4,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.StudentFeeProfile',
    },
    {
      path: '/student-fee-discounts',
      name: '::Menu:StudentFeeDiscounts',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-percent',
      order: 5,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.StudentFeeDiscount',
    },
    {
      path: '/late-fee-polices',
      name: '::Menu:LateFeePolicies',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-clock',
      order: 6,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.LateFeePolicie',
    },
    {
      path: '/student-monthly-fees',
      name: '::Menu:StudentMonthlyFees',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-calendar-alt',
      order: 7,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.StudentMonthlyFee',
    },
    {
      path: '/student-monthly-fee-lines',
      name: '::Menu:StudentMonthlyFeeLines',
      parentName: '::Menu:Fees',
      iconClass: 'fas fa-list-ul',
      order: 8,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.StudentMonthlyFeeLine',
    },
    {
      path: '/check-fees-dashboard',
      name: '::Menu:CheckFees',
      parentName: '::Menu:Fees',
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.FeeMenu.CheckFee',
      order: 10,
      iconClass: 'fas fa-receipt',
    },

    // -------------------------
    // Expenses (Parent)
    // -------------------------
    {
      name: '::Menu:Expenses',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-file-invoice-dollar',
      order: 50,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Expenses',
    },
    {
      path: '/expense-categories',
      name: '::Menu:ExpenseCategories',
      parentName: '::Menu:Expenses',
      iconClass: 'fas fa-list',
      order: 1,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Expenses.ExpenseCategory',
    },
    {
      path: '/expense-entries',
      name: '::Menu:ExpenseEntry',
      parentName: '::Menu:Expenses',
      iconClass: 'fas fa-receipt',
      order: 2,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Expenses.ExpenseEntry',
    },
    {
      path: '/staff-salary-payments',
      name: '::Menu:StaffSalaryPayments',
      parentName: '::Menu:Expenses',
      iconClass: 'fas fa-money-check-alt',
      order: 3,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Expenses.StaffSalaryPayment',
    },
    {
      path: '/expense-dashboard',
      name: '::Menu:ExpensesDashboard',
      parentName: '::Menu:Expenses',
      iconClass: 'fas fa-chart-pie',
      order: 4,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Expenses.ExpenseDashboard',
    },
    {
      path: 'reports/report',
      name: '::Menu:Reports',
      parentName: '::SchoolManagement',
      iconClass: 'fas fa-chart-pie',
      order: 60,
      layout: eLayoutType.application,
      requiredPolicy: 'EHub.Report',
    },
  ]);
}
