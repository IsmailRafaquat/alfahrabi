// ONE centralized bilingual (English/Urdu) label dictionary for every printable receipt/document,
// shared by every ShopPrintLayoutComponent instance instead of each document/page hand-rolling
// its own copy (the pre-existing pattern in shop-sale-detail.component.ts, now retired in favour
// of this shared dictionary - see the sale-detail migration).
export type ShopPrintLang = 'en' | 'ur';

export interface ShopPrintLabelSet {
  documentTitle: (documentType: string) => string;
  invoice: string;
  date: string;
  cashier: string;
  customer: string;
  supplier: string;
  walkInCustomer: string;
  phone: string;
  item: string;
  qty: string;
  rate: string;
  amount: string;
  batch: string;
  exp: string;
  subtotal: string;
  discount: string;
  tax: string;
  otherCharges: string;
  adjustment: string;
  total: string;
  paid: string;
  pending: string;
  change: string;
  paymentMethod: string;
  received: string;
  reference: string;
  notes: string;
  thankYou: string;
  customerCopy: string;
  duplicateCopy: string;
}

const DOCUMENT_TITLES_EN: Record<string, string> = {
  sale: 'SALE RECEIPT',
  'sale-return': 'SALE RETURN RECEIPT',
  'purchase-order': 'PURCHASE ORDER',
  'goods-receipt': 'GOODS RECEIPT',
  'purchase-return': 'PURCHASE RETURN',
  'customer-payment': 'CUSTOMER PAYMENT RECEIPT',
  'supplier-payment': 'SUPPLIER PAYMENT RECEIPT',
  'customer-ledger-statement': 'CUSTOMER LEDGER STATEMENT',
  'supplier-ledger-statement': 'SUPPLIER LEDGER STATEMENT',
  'expense-voucher': 'EXPENSE VOUCHER',
  'cash-closing-slip': 'CASH CLOSING SLIP',
  'bank-transaction': 'BANK TRANSACTION RECEIPT',
  'bank-transfer': 'BANK TRANSFER RECEIPT',
  'stock-adjustment': 'STOCK ADJUSTMENT SLIP',
  'stock-count': 'PHYSICAL STOCK COUNT SUMMARY',
  'stock-transaction': 'STOCK TRANSACTION',
  'product-barcode-label': 'PRODUCT LABEL',
  'batch-expiry-label': 'BATCH LABEL',
};

const DOCUMENT_TITLES_UR: Record<string, string> = {
  sale: 'سیل رسید',
  'sale-return': 'سیل ریٹرن رسید',
  'purchase-order': 'خریداری آرڈر',
  'goods-receipt': 'گڈز رسیپٹ',
  'purchase-return': 'خریداری واپسی',
  'customer-payment': 'کسٹمر ادائیگی رسید',
  'supplier-payment': 'سپلائر ادائیگی رسید',
  'customer-ledger-statement': 'کسٹمر لیجر اسٹیٹمنٹ',
  'supplier-ledger-statement': 'سپلائر لیجر اسٹیٹمنٹ',
  'expense-voucher': 'اخراجات واؤچر',
  'cash-closing-slip': 'کیش کلوزنگ سلپ',
  'bank-transaction': 'بینک ٹرانزیکشن رسید',
  'bank-transfer': 'بینک ٹرانسفر رسید',
  'stock-adjustment': 'اسٹاک ایڈجسٹمنٹ سلپ',
  'stock-count': 'فزیکل اسٹاک کاؤنٹ خلاصہ',
  'stock-transaction': 'اسٹاک ٹرانزیکشن',
  'product-barcode-label': 'پروڈکٹ لیبل',
  'batch-expiry-label': 'بیچ لیبل',
};

export const SHOP_PRINT_LABELS: Record<ShopPrintLang, ShopPrintLabelSet> = {
  en: {
    documentTitle: (documentType) => DOCUMENT_TITLES_EN[documentType] ?? 'RECEIPT',
    invoice: 'Invoice',
    date: 'Date',
    cashier: 'Cashier',
    customer: 'Customer',
    supplier: 'Supplier',
    walkInCustomer: 'Walk-in Customer',
    phone: 'Phone',
    item: 'Item',
    qty: 'Qty',
    rate: 'Rate',
    amount: 'Amount',
    batch: 'Batch',
    exp: 'Exp',
    subtotal: 'Subtotal',
    discount: 'Discount',
    tax: 'Tax',
    otherCharges: 'Other Charges',
    adjustment: 'Adjustment',
    total: 'TOTAL',
    paid: 'Paid',
    pending: 'Pending',
    change: 'Change',
    paymentMethod: 'Payment Method',
    received: 'Received',
    reference: 'Reference',
    notes: 'Notes',
    thankYou: 'Thank you for shopping with us.',
    customerCopy: 'Customer Copy',
    duplicateCopy: 'Duplicate Copy',
  },
  ur: {
    documentTitle: (documentType) => DOCUMENT_TITLES_UR[documentType] ?? 'رسید',
    invoice: 'انوائس',
    date: 'تاریخ',
    cashier: 'کیشیئر',
    customer: 'کسٹمر',
    supplier: 'سپلائر',
    walkInCustomer: 'واک اِن کسٹمر',
    phone: 'فون',
    item: 'آئٹم',
    qty: 'مقدار',
    rate: 'ریٹ',
    amount: 'رقم',
    batch: 'بیچ',
    exp: 'میعاد',
    subtotal: 'سب ٹوٹل',
    discount: 'رعایت',
    tax: 'ٹیکس',
    otherCharges: 'دیگر اخراجات',
    adjustment: 'ایڈجسٹمنٹ',
    total: 'ٹوٹل',
    paid: 'ادا شدہ',
    pending: 'زیر التوا',
    change: 'باقی رقم',
    paymentMethod: 'ادائیگی کا طریقہ',
    received: 'وصول شدہ',
    reference: 'حوالہ',
    notes: 'نوٹس',
    thankYou: 'خریداری کا شکریہ۔',
    customerCopy: 'کسٹمر کاپی',
    duplicateCopy: 'ڈپلیکیٹ کاپی',
  },
};
