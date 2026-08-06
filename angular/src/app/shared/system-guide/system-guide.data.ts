import { ModuleGuide } from './system-guide.models';

/**
 * Source of truth for every entry here: route.provider.ts's shopManagementRoutes (routes, icons,
 * requiredPolicy strings) plus the actual DTOs/entities/AppServices behind each route - not guessed.
 * One entry per top-level Shop Management menu group, matching the sidebar's own grouping so this
 * guide never describes a workflow the app doesn't actually have.
 */
export const SYSTEM_GUIDE_MODULES: ModuleGuide[] = [
  {
    key: 'dashboard',
    icon: 'fas fa-tachometer-alt',
    route: '/shop-management/dashboard',
    requiredPolicy: 'ShopManagement.Dashboard',
    name: { en: 'Dashboard', ur: 'ڈیش بورڈ' },
    shortDescription: {
      en: 'A quick overview of today\'s sales, cash position, and alerts.',
      ur: 'آج کی فروخت، نقدی کی صورتحال اور اہم اطلاعات کا فوری جائزہ۔',
    },
    purpose: {
      en: 'Gives a single-page snapshot of the shop\'s activity so you don\'t have to open every module to see how the day is going.',
      ur: 'دکان کی سرگرمی کا ایک ہی صفحے پر جائزہ فراہم کرتا ہے تاکہ ہر ماڈیول الگ سے کھولنے کی ضرورت نہ پڑے۔',
    },
    whenToUse: {
      en: 'At the start of the day, or any time you need a fast summary before deciding what to check next.',
      ur: 'دن کے آغاز پر، یا جب بھی اگلا اقدام طے کرنے سے پہلے فوری خلاصہ درکار ہو۔',
    },
    requiredInformation: { en: [], ur: [] },
    workflow: {
      en: [
        'Open Dashboard from the sidebar.',
        'Review the summary cards (sales, cash, low stock, dues).',
        'Click through a card to open the related module for more detail.',
      ],
      ur: [
        'سائیڈبار سے ڈیش بورڈ کھولیں۔',
        'خلاصہ کارڈز (فروخت، نقدی، کم اسٹاک، بقایا جات) کا جائزہ لیں۔',
        'مزید تفصیل کے لیے کسی کارڈ پر کلک کر کے متعلقہ ماڈیول کھولیں۔',
      ],
    },
    whatHappensNext: {
      en: 'Nothing is created or changed here - it only reads existing data from other modules.',
      ur: 'یہاں کوئی نیا ریکارڈ نہیں بنتا - یہ صرف دیگر ماڈیولز کا موجودہ ڈیٹا دکھاتا ہے۔',
    },
    relatedModules: ['sales', 'cashManagement', 'notifications', 'profitLoss'],
    importantNotes: {
      en: ['This page is read-only; use the linked module to make changes.'],
      ur: ['یہ صفحہ صرف دیکھنے کے لیے ہے؛ تبدیلی کے لیے متعلقہ ماڈیول استعمال کریں۔'],
    },
  },
  {
    key: 'settings',
    icon: 'fas fa-cog',
    route: '/shop-management/settings',
    requiredPolicy: 'ShopManagement.Settings',
    name: { en: 'Settings', ur: 'سیٹنگز' },
    shortDescription: {
      en: 'Configure the shop profile, currency, tax, and invoice/receipt defaults used everywhere else.',
      ur: 'دکان کا پروفائل، کرنسی، ٹیکس اور انوائس/رسید کی ڈیفالٹ ترتیبات یہاں سیٹ کریں۔',
    },
    purpose: {
      en: 'One place to set the shop\'s identity and the numbering/tax defaults every other module reads from.',
      ur: 'دکان کی شناخت اور نمبرنگ/ٹیکس کی ڈیفالٹ ترتیبات ایک ہی جگہ سیٹ کرنے کے لیے، جنہیں باقی تمام ماڈیولز استعمال کرتے ہیں۔',
    },
    whenToUse: {
      en: 'Before starting to use the system, and whenever shop details, currency, or tax rules change.',
      ur: 'سسٹم استعمال شروع کرنے سے پہلے، اور جب بھی دکان کی تفصیلات، کرنسی یا ٹیکس کے اصول تبدیل ہوں۔',
    },
    requiredInformation: {
      en: ['Shop display name and logo', 'Contact and address details', 'Currency code and symbol', 'Default tax percentage and tax number', 'Invoice and purchase order number prefixes'],
      ur: ['دکان کا نام اور لوگو', 'رابطہ اور پتے کی تفصیلات', 'کرنسی کوڈ اور علامت', 'ڈیفالٹ ٹیکس فیصد اور ٹیکس نمبر', 'انوائس اور پرچیز آرڈر نمبر کا پری فکس'],
    },
    workflow: {
      en: [
        'Open Settings.',
        'Fill in the shop profile (name, logo, contact, address).',
        'Set the currency code and symbol.',
        'Set the default tax percentage, invoice prefix, and purchase order prefix.',
        'Optionally set receipt footer text, return policy, low-stock default, and whether negative stock is allowed.',
        'Save.',
      ],
      ur: [
        'سیٹنگز کھولیں۔',
        'شاپ پروفائل (نام، لوگو، رابطہ، پتہ) مکمل کریں۔',
        'کرنسی کوڈ اور علامت منتخب کریں۔',
        'ڈیفالٹ ٹیکس فیصد، انوائس پری فکس اور پرچیز آرڈر پری فکس سیٹ کریں۔',
        'اختیاری طور پر رسید کا فوٹر، ریٹرن پالیسی، کم اسٹاک کی ڈیفالٹ سطح اور منفی اسٹاک کی اجازت سیٹ کریں۔',
        'محفوظ کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'These values become the defaults used across Products, Sales, Purchases, and printed documents.',
      ur: 'یہ ترتیبات پروڈکٹس، سیلز، خریداری اور پرنٹ ہونے والی دستاویزات میں بطور ڈیفالٹ استعمال ہوتی ہیں۔',
    },
    relatedModules: ['products', 'sales', 'purchases'],
    importantNotes: {
      en: ['The shop must be marked as configured (IsConfigured) before it is considered fully set up.'],
      ur: ['شاپ کو مکمل طور پر تیار سمجھنے کے لیے "IsConfigured" کی حیثیت درست ہونی چاہیے۔'],
    },
  },
  {
    key: 'products',
    icon: 'fas fa-boxes',
    route: '/shop-management/products',
    requiredPolicy: 'ShopManagement.ProductCategories',
    name: { en: 'Products', ur: 'پروڈکٹس' },
    shortDescription: {
      en: 'Add and manage products, categories, units, prices, and stock information.',
      ur: 'مصنوعات، کیٹیگریز، یونٹس، قیمتیں اور اسٹاک کی معلومات شامل اور منظم کریں۔',
    },
    purpose: {
      en: 'This is where every item the shop buys or sells is defined, along with its pricing and stock tracking rules.',
      ur: 'یہاں ہر وہ چیز درج کی جاتی ہے جو دکان خریدتی یا فروخت کرتی ہے، اس کی قیمتوں اور اسٹاک ٹریکنگ کے قواعد کے ساتھ۔',
    },
    whenToUse: {
      en: 'Before a product can be purchased or sold, it must first be added here - together with its category and unit.',
      ur: 'کسی بھی پروڈکٹ کی خریداری یا فروخت سے پہلے اسے یہاں، اس کی کیٹیگری اور یونٹ کے ساتھ شامل کرنا ضروری ہے۔',
    },
    requiredInformation: {
      en: ['Product category', 'Unit of measure (e.g. piece, kg)', 'Product name and code', 'Purchase price and sale price', 'Minimum stock level and reorder level', 'Whether the product is batch/expiry/serial tracked'],
      ur: ['پروڈکٹ کیٹیگری', 'پیمائش کا یونٹ (مثلاً پیس، کلوگرام)', 'پروڈکٹ کا نام اور کوڈ', 'خرید اور فروخت کی قیمت', 'کم از کم اور ری آرڈر اسٹاک کی سطح', 'کیا پروڈکٹ بیچ/میعاد ختم ہونے کی تاریخ/سیریل نمبر سے ٹریک ہوگا'],
    },
    workflow: {
      en: [
        'Open Products.',
        'Create a Product Category and a Unit first, if they don\'t already exist.',
        'Click Add Product.',
        'Select the product category and unit.',
        'Enter product name, code, purchase price, sale price, and stock levels.',
        'Save the product.',
        'The product becomes available for use in Purchases, Sales, and Inventory.',
      ],
      ur: [
        'پروڈکٹس ماڈیول کھولیں۔',
        'اگر پہلے سے موجود نہیں تو پہلے پروڈکٹ کیٹیگری اور یونٹ بنائیں۔',
        'ایڈ پروڈکٹ پر کلک کریں۔',
        'پروڈکٹ کیٹیگری اور یونٹ منتخب کریں۔',
        'پروڈکٹ کا نام، کوڈ، خرید اور فروخت کی قیمت اور اسٹاک کی سطحیں درج کریں۔',
        'پروڈکٹ محفوظ کریں۔',
        'محفوظ ہونے کے بعد پروڈکٹ خریداری، فروخت اور انوینٹری میں دستیاب ہو جاتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'The product appears in the Sales and Purchases product pickers, and its stock is tracked in Inventory.',
      ur: 'پروڈکٹ سیلز اور خریداری کی فہرست میں دستیاب ہو جاتا ہے، اور اس کا اسٹاک انوینٹری میں ٹریک ہوتا ہے۔',
    },
    relatedModules: ['purchases', 'sales', 'inventory'],
    importantNotes: {
      en: ['Purchase price is only a reference - the actual cost used for Profit & Loss comes from the price recorded on each purchase/sale.'],
      ur: ['خرید کی قیمت صرف حوالے کے لیے ہے - منافع و نقصان میں اصل لاگت ہر خریداری/فروخت پر درج شدہ قیمت سے لی جاتی ہے۔'],
    },
  },
  {
    key: 'suppliers',
    icon: 'fas fa-truck',
    route: '/shop-management/suppliers',
    requiredPolicy: 'ShopManagement.Suppliers',
    name: { en: 'Suppliers', ur: 'سپلائرز' },
    shortDescription: {
      en: 'Manage supplier contact details, opening balance, purchases, and outstanding payables.',
      ur: 'سپلائرز کی رابطہ تفصیلات، ابتدائی بیلنس، خریداری اور بقایا ادائیگیوں کا انتظام کریں۔',
    },
    purpose: {
      en: 'Keeps a record of everyone the shop buys stock from, and how much is owed to each of them.',
      ur: 'ہر اس شخص کا ریکارڈ رکھتا ہے جس سے دکان مال خریدتی ہے، اور ہر ایک کو کتنی رقم واجب الادا ہے۔',
    },
    whenToUse: {
      en: 'Add a supplier before creating a purchase order against them.',
      ur: 'کسی سپلائر کے خلاف پرچیز آرڈر بنانے سے پہلے اسے یہاں شامل کریں۔',
    },
    requiredInformation: {
      en: ['Supplier code and name', 'Contact details (phone, address)', 'Opening balance, if any'],
      ur: ['سپلائر کوڈ اور نام', 'رابطہ تفصیلات (فون، پتہ)', 'اگر کوئی ہو تو ابتدائی بیلنس'],
    },
    workflow: {
      en: [
        'Open Suppliers.',
        'Click Add Supplier.',
        'Enter code, name, contact details, and opening balance.',
        'Save.',
        'Use Purchase Orders to buy from this supplier; the Supplier Ledger then shows every purchase, payment, and the running balance.',
      ],
      ur: [
        'سپلائرز ماڈیول کھولیں۔',
        'ایڈ سپلائر پر کلک کریں۔',
        'کوڈ، نام، رابطہ تفصیلات اور ابتدائی بیلنس درج کریں۔',
        'محفوظ کریں۔',
        'اس سپلائر سے خریداری کے لیے پرچیز آرڈر استعمال کریں؛ سپلائر لیجر ہر خریداری، ادائیگی اور بقایا بیلنس دکھاتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'The supplier appears in the Purchase Order picker; every completed purchase and payment updates their pending amount.',
      ur: 'سپلائر پرچیز آرڈر کی فہرست میں دستیاب ہو جاتا ہے؛ ہر مکمل خریداری اور ادائیگی ان کی بقایا رقم کو اپڈیٹ کرتی ہے۔',
    },
    relatedModules: ['purchases'],
    importantNotes: {
      en: ['Supplier Ledger and pending amount are viewed from the Purchases area, not from the Suppliers list itself.'],
      ur: ['سپلائر لیجر اور بقایا رقم پرچیزز کے سیکشن سے دیکھی جاتی ہے، سپلائرز کی فہرست سے نہیں۔'],
    },
  },
  {
    key: 'customers',
    icon: 'fas fa-users',
    route: '/shop-management/customers',
    requiredPolicy: 'ShopManagement.Customers',
    name: { en: 'Customers', ur: 'کسٹمرز' },
    shortDescription: {
      en: 'Manage customer contact information, credit balance, sales, and ledger.',
      ur: 'کسٹمرز کی رابطہ معلومات، کریڈٹ بیلنس، فروخت اور لیجر کا انتظام کریں۔',
    },
    purpose: {
      en: 'Keeps a record of everyone the shop sells to on credit or with a running account, and what they owe.',
      ur: 'ہر اس کسٹمر کا ریکارڈ رکھتا ہے جسے دکان ادھار یا چلتے کھاتے پر فروخت کرتی ہے، اور اس پر کتنی رقم واجب الادا ہے۔',
    },
    whenToUse: {
      en: 'Add a customer before recording a credit sale, or to track a walk-in customer\'s purchase history.',
      ur: 'ادھار فروخت درج کرنے سے پہلے، یا کسٹمر کی خریداری کی تاریخ ٹریک کرنے کے لیے کسٹمر شامل کریں۔',
    },
    requiredInformation: {
      en: ['Customer code and name', 'Customer type (e.g. business or individual)', 'Contact details', 'Opening balance and payment terms, if any'],
      ur: ['کسٹمر کوڈ اور نام', 'کسٹمر کی قسم (مثلاً کاروبار یا انفرادی)', 'رابطہ تفصیلات', 'اگر کوئی ہو تو ابتدائی بیلنس اور ادائیگی کی مدت'],
    },
    workflow: {
      en: [
        'Open Customers.',
        'Click Add Customer.',
        'Enter code, name, type, contact details, and opening balance.',
        'Save.',
        'Use Sales to sell to this customer; the Customer Ledger then shows every sale, payment, and the running balance.',
      ],
      ur: [
        'کسٹمرز ماڈیول کھولیں۔',
        'ایڈ کسٹمر پر کلک کریں۔',
        'کوڈ، نام، قسم، رابطہ تفصیلات اور ابتدائی بیلنس درج کریں۔',
        'محفوظ کریں۔',
        'اس کسٹمر کو فروخت کے لیے سیلز استعمال کریں؛ کسٹمر لیجر ہر فروخت، ادائیگی اور بقایا بیلنس دکھاتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'The customer appears in the Sales picker; credit sales and payments update their outstanding balance.',
      ur: 'کسٹمر سیلز کی فہرست میں دستیاب ہو جاتا ہے؛ ادھار فروخت اور ادائیگیاں ان کا بقایا بیلنس اپڈیٹ کرتی ہیں۔',
    },
    relatedModules: ['sales'],
    importantNotes: {
      en: ['A walk-in/cash sale does not require selecting a customer in most cases.'],
      ur: ['زیادہ تر صورتوں میں واک اِن یا نقد فروخت کے لیے کسٹمر منتخب کرنا ضروری نہیں۔'],
    },
  },
  {
    key: 'sales',
    icon: 'fas fa-cash-register',
    route: '/shop-management/sales',
    requiredPolicy: 'ShopManagement.Sales',
    name: { en: 'Sales', ur: 'سیلز' },
    shortDescription: {
      en: 'Record sales (point of sale), customer payments, the customer ledger, and sale returns.',
      ur: 'فروخت (پوائنٹ آف سیل)، کسٹمر ادائیگیاں، کسٹمر لیجر اور سیل ریٹرن درج کریں۔',
    },
    purpose: {
      en: 'Covers the whole selling process: creating a sale, taking payment, printing the invoice, and handling returns.',
      ur: 'پوری فروخت کے عمل کا احاطہ کرتا ہے: سیل بنانا، ادائیگی وصول کرنا، انوائس پرنٹ کرنا اور واپسی سنبھالنا۔',
    },
    whenToUse: {
      en: 'Every time a product is sold to a customer.',
      ur: 'جب بھی کسٹمر کو کوئی پروڈکٹ فروخت کی جائے۔',
    },
    requiredInformation: {
      en: ['Customer (optional for a cash sale)', 'Products, quantity, and unit price', 'Discount and/or tax, if applicable', 'Payment method and amount paid'],
      ur: ['کسٹمر (نقد فروخت کے لیے اختیاری)', 'پروڈکٹس، مقدار اور یونٹ قیمت', 'اگر لاگو ہو تو ڈسکاؤنٹ اور/یا ٹیکس', 'ادائیگی کا طریقہ اور ادا شدہ رقم'],
    },
    workflow: {
      en: [
        'Open Sales.',
        'Select a customer, or leave it as a walk-in/cash sale.',
        'Add products with quantity and price.',
        'Apply discount/tax if needed.',
        'Enter the payment amount and method.',
        'Complete the sale to generate the invoice.',
        'Stock is reduced and, for credit sales, the Customer Ledger is updated.',
      ],
      ur: [
        'سیلز ماڈیول کھولیں۔',
        'کسٹمر منتخب کریں، یا نقد/واک اِن فروخت کے طور پر چھوڑ دیں۔',
        'پروڈکٹس مقدار اور قیمت کے ساتھ شامل کریں۔',
        'ضرورت ہو تو ڈسکاؤنٹ/ٹیکس لگائیں۔',
        'ادائیگی کی رقم اور طریقہ درج کریں۔',
        'انوائس بنانے کے لیے فروخت مکمل کریں۔',
        'اسٹاک کم ہو جاتا ہے اور ادھار فروخت کی صورت میں کسٹمر لیجر اپڈیٹ ہوتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'A completed sale reduces product stock, is counted in Profit & Loss revenue, and (if unpaid or partially paid) shows up as a customer receivable. It can later be reversed with a Sale Return.',
      ur: 'مکمل شدہ فروخت پروڈکٹ کا اسٹاک کم کرتی ہے، منافع و نقصان کی آمدنی میں شمار ہوتی ہے، اور اگر ادائیگی نامکمل ہو تو کسٹمر کی وصولی میں ظاہر ہوتی ہے۔ بعد میں سیل ریٹرن کے ذریعے واپس کی جا سکتی ہے۔',
    },
    relatedModules: ['customers', 'products', 'inventory', 'cashManagement', 'profitLoss'],
    importantNotes: {
      en: ['Only a Completed sale affects stock and Profit & Loss - a Draft sale does not.', 'Sale Returns and Sale Returns require selecting the original sale and item.'],
      ur: ['اسٹاک اور منافع و نقصان پر صرف مکمل شدہ فروخت اثر انداز ہوتی ہے - ڈرافٹ فروخت نہیں۔', 'سیل ریٹرن کے لیے اصل فروخت اور آئٹم منتخب کرنا ضروری ہے۔'],
    },
  },
  {
    key: 'purchases',
    icon: 'fas fa-file-invoice',
    route: '/shop-management/purchase-orders',
    requiredPolicy: 'ShopManagement.PurchaseOrders',
    name: { en: 'Purchases', ur: 'خریداری' },
    shortDescription: {
      en: 'Manage purchase orders, goods receipts, supplier payments, purchase returns, and the supplier ledger.',
      ur: 'پرچیز آرڈرز، گڈز رسیپٹ، سپلائر ادائیگیاں، خریداری کی واپسی اور سپلائر لیجر کا انتظام کریں۔',
    },
    purpose: {
      en: 'Covers the full buying process: ordering stock from a supplier, receiving it into inventory, and paying for it.',
      ur: 'مکمل خریداری کے عمل کا احاطہ کرتا ہے: سپلائر سے مال منگوانا، اسے انوینٹری میں وصول کرنا اور اس کی ادائیگی کرنا۔',
    },
    whenToUse: {
      en: 'Whenever the shop needs to restock products from a supplier.',
      ur: 'جب بھی دکان کو سپلائر سے پروڈکٹس کا اسٹاک دوبارہ منگوانا ہو۔',
    },
    requiredInformation: {
      en: ['Supplier', 'Products, ordered quantity, and purchase price', 'Received quantity at goods receipt', 'Payment method and amount, when paying the supplier'],
      ur: ['سپلائر', 'پروڈکٹس، آرڈر کی گئی مقدار اور خرید قیمت', 'گڈز رسیپٹ پر وصول شدہ مقدار', 'سپلائر کو ادائیگی کرتے وقت ادائیگی کا طریقہ اور رقم'],
    },
    workflow: {
      en: [
        'Select a Supplier (add one first if needed).',
        'Create a Purchase Order with the products, quantities, and prices.',
        'Record a Goods Receipt against the purchase order when stock physically arrives.',
        'Stock is increased in Inventory as items are received.',
        'The Supplier Ledger records the purchase.',
        'Record a Supplier Payment to pay part or all of the amount owed.',
        'If items need to be returned, use Purchase Returns.',
      ],
      ur: [
        'سپلائر منتخب کریں (اگر ضروری ہو تو پہلے شامل کریں)۔',
        'پروڈکٹس، مقدار اور قیمتوں کے ساتھ پرچیز آرڈر بنائیں۔',
        'جب مال حقیقتاً پہنچے تو پرچیز آرڈر کے خلاف گڈز رسیپٹ درج کریں۔',
        'وصول شدہ اشیاء کے مطابق انوینٹری میں اسٹاک بڑھ جاتا ہے۔',
        'سپلائر لیجر میں خریداری درج ہو جاتی ہے۔',
        'واجب الادا رقم کا کچھ حصہ یا مکمل ادائیگی کے لیے سپلائر پیمنٹ درج کریں۔',
        'اگر اشیاء واپس کرنی ہوں تو پرچیز ریٹرن استعمال کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'Supplier → Purchase Order → Goods Receipt → Stock Increase → Supplier Ledger → Payment/Pending Amount. The received cost also becomes the cost basis used for Cost of Goods Sold.',
      ur: 'سپلائر → پرچیز آرڈر → گڈز رسیپٹ → اسٹاک میں اضافہ → سپلائر لیجر → ادائیگی یا بقایا رقم۔ وصول شدہ لاگت ہی فروخت شدہ سامان کی لاگت کا حساب لگانے میں استعمال ہوتی ہے۔',
    },
    relatedModules: ['suppliers', 'products', 'inventory', 'cashManagement'],
    importantNotes: {
      en: ['Stock only increases once a Goods Receipt is completed - creating the Purchase Order alone does not change stock.'],
      ur: ['اسٹاک صرف گڈز رسیپٹ مکمل ہونے پر بڑھتا ہے - صرف پرچیز آرڈر بنانے سے اسٹاک تبدیل نہیں ہوتا۔'],
    },
  },
  {
    key: 'inventory',
    icon: 'fas fa-warehouse',
    route: '/shop-management/stock-transactions',
    requiredPolicy: 'ShopManagement.StockTransactions',
    name: { en: 'Inventory', ur: 'انوینٹری' },
    shortDescription: {
      en: 'Track current stock, stock movements, manual adjustments, and physical stock counts.',
      ur: 'موجودہ اسٹاک، اسٹاک کی نقل و حرکت، دستی ایڈجسٹمنٹ اور فزیکل اسٹاک شماری ٹریک کریں۔',
    },
    purpose: {
      en: 'Shows how much of each product is actually in stock, and lets you correct it when the physical count doesn\'t match the system.',
      ur: 'ہر پروڈکٹ کا اصل موجودہ اسٹاک دکھاتا ہے، اور جب فزیکل شماری سسٹم سے مطابقت نہ رکھے تو اسے درست کرنے دیتا ہے۔',
    },
    whenToUse: {
      en: 'To review stock movement history, correct stock after damage/loss, or perform a periodic physical count.',
      ur: 'اسٹاک کی نقل و حرکت کی تاریخ دیکھنے، نقصان کے بعد اسٹاک درست کرنے، یا وقتاً فوقتاً فزیکل شماری کے لیے۔',
    },
    requiredInformation: {
      en: ['Product and quantity for an adjustment', 'Reason for the adjustment (e.g. damaged, lost)', 'Counted quantity for a physical stock count'],
      ur: ['ایڈجسٹمنٹ کے لیے پروڈکٹ اور مقدار', 'ایڈجسٹمنٹ کی وجہ (مثلاً خراب، گم شدہ)', 'فزیکل اسٹاک شماری کے لیے گنی گئی مقدار'],
    },
    workflow: {
      en: [
        'Open Inventory.',
        'Stock Transactions shows every stock-in/stock-out movement, generated automatically by Goods Receipts, Sales, Sale Returns, and Purchase Returns.',
        'For a manual correction, use Stock Adjustments: select the product, enter the correct quantity and a reason, and save.',
        'For a periodic count, use Stock Counts: enter the physically counted quantity per product; the system records the difference against expected stock.',
        'Batch-tracked products can be reviewed in Product Batches, including near-expiry and expired batches.',
      ],
      ur: [
        'انوینٹری ماڈیول کھولیں۔',
        'اسٹاک ٹرانزیکشنز ہر اسٹاک اِن/آؤٹ نقل و حرکت دکھاتی ہے، جو گڈز رسیپٹ، سیلز، سیل ریٹرن اور پرچیز ریٹرن سے خودکار طور پر بنتی ہے۔',
        'دستی درستگی کے لیے اسٹاک ایڈجسٹمنٹ استعمال کریں: پروڈکٹ منتخب کریں، درست مقدار اور وجہ درج کریں اور محفوظ کریں۔',
        'وقتاً فوقتاً شماری کے لیے اسٹاک کاؤنٹس استعمال کریں: ہر پروڈکٹ کی فزیکل گنی گئی مقدار درج کریں؛ سسٹم متوقع اسٹاک سے فرق ریکارڈ کرتا ہے۔',
        'بیچ سے ٹریک ہونے والی پروڈکٹس کو پروڈکٹ بیچز میں دیکھا جا سکتا ہے، بشمول قریب المیعاد اور میعاد ختم شدہ بیچز۔',
      ],
    },
    whatHappensNext: {
      en: 'Adjustments and stock counts directly change the quantity available for sale.',
      ur: 'ایڈجسٹمنٹس اور اسٹاک شماری فروخت کے لیے دستیاب مقدار کو براہ راست تبدیل کرتی ہیں۔',
    },
    relatedModules: ['products', 'purchases', 'sales'],
    importantNotes: {
      en: ['Low-stock and near-expiry situations are surfaced automatically in Notifications.'],
      ur: ['کم اسٹاک اور قریب المیعاد صورتحال خودکار طور پر اطلاعات میں دکھائی جاتی ہے۔'],
    },
  },
  {
    key: 'expenses',
    icon: 'fas fa-file-invoice-dollar',
    route: '/shop-management/expenses',
    requiredPolicy: 'ShopManagement.Expenses',
    name: { en: 'Expenses', ur: 'اخراجات' },
    shortDescription: {
      en: 'Record and categorize operating expenses such as rent, utilities, and salaries.',
      ur: 'کرایہ، بجلی/گیس اور تنخواہوں جیسے آپریٹنگ اخراجات درج اور درجہ بند کریں۔',
    },
    purpose: {
      en: 'Tracks everything the shop spends outside of buying stock, categorized so it can be reviewed and reported on.',
      ur: 'ہر وہ خرچہ ٹریک کرتا ہے جو دکان مال کی خریداری کے علاوہ کرتی ہے، تاکہ اس کا جائزہ اور رپورٹنگ ممکن ہو۔',
    },
    whenToUse: {
      en: 'Any time the shop pays for something that is not a product purchase.',
      ur: 'جب بھی دکان کسی ایسی چیز کی ادائیگی کرے جو پروڈکٹ کی خریداری نہ ہو۔',
    },
    requiredInformation: {
      en: ['Expense category', 'Expense date', 'Amount', 'Payment method', 'Description'],
      ur: ['اخراجات کی کیٹیگری', 'اخراجات کی تاریخ', 'رقم', 'ادائیگی کا طریقہ', 'تفصیل'],
    },
    workflow: {
      en: [
        'Create an Expense Category if the right one doesn\'t exist yet (e.g. Rent, Utilities).',
        'Open Expenses and click Add Expense.',
        'Select the category, date, amount, and payment method.',
        'Enter a description.',
        'Post the expense.',
      ],
      ur: [
        'اگر مناسب کیٹیگری موجود نہیں تو پہلے اخراجات کیٹیگری بنائیں (مثلاً کرایہ، یوٹیلیٹیز)۔',
        'اخراجات ماڈیول کھولیں اور ایڈ ایکسپینس پر کلک کریں۔',
        'کیٹیگری، تاریخ، رقم اور ادائیگی کا طریقہ منتخب کریں۔',
        'تفصیل درج کریں۔',
        'خرچہ پوسٹ کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'A posted expense reduces cash (if paid in cash) and is counted as an operating expense in Profit & Loss.',
      ur: 'پوسٹ شدہ خرچہ نقدی کم کرتا ہے (اگر نقد ادا کیا گیا ہو) اور منافع و نقصان میں آپریٹنگ اخراجات میں شمار ہوتا ہے۔',
    },
    relatedModules: ['cashManagement', 'profitLoss'],
    importantNotes: {
      en: ['Only a Posted expense affects cash and Profit & Loss.'],
      ur: ['نقدی اور منافع و نقصان پر صرف پوسٹ شدہ خرچہ اثر انداز ہوتا ہے۔'],
    },
  },
  {
    key: 'cashManagement',
    icon: 'fas fa-cash-register',
    route: '/shop-management/cash-register',
    requiredPolicy: 'ShopManagement.CashRegisters',
    name: { en: 'Cash Management', ur: 'کیش مینجمنٹ' },
    shortDescription: {
      en: 'Open/close the cash register, review cash transactions, and reconcile expected vs. actual cash.',
      ur: 'کیش رجسٹر کھولیں/بند کریں، کیش لین دین دیکھیں اور متوقع و اصل کیش کا موازنہ کریں۔',
    },
    purpose: {
      en: 'Tracks physical cash in the register through the day and reconciles it against everything the system expects to be there.',
      ur: 'دن بھر رجسٹر میں موجود اصل نقدی کو ٹریک کرتا ہے اور اسے سسٹم کے متوقع حساب سے ملاتا ہے۔',
    },
    whenToUse: {
      en: 'At the start of the day to open the register, and at the end of the day to close it and reconcile cash.',
      ur: 'دن کے آغاز پر رجسٹر کھولنے کے لیے، اور دن کے اختتام پر اسے بند اور کیش کا موازنہ کرنے کے لیے۔',
    },
    requiredInformation: {
      en: ['Opening cash amount', 'Actual closing cash counted at the end of the day'],
      ur: ['ابتدائی کیش کی رقم', 'دن کے اختتام پر گنی گئی اصل اختتامی کیش'],
    },
    workflow: {
      en: [
        'Open the Cash Register and record the opening cash for the day.',
        'Cash Sales, Customer Cash Payments, and any manual cash-in add to the register.',
        'Supplier Cash Payments, Cash Expenses, Customer Refunds, and any manual cash-out reduce it.',
        'At day end, open Daily Closings and enter the actual counted cash.',
        'The system compares Expected Closing Cash against Actual Closing Cash and shows the difference.',
      ],
      ur: [
        'کیش رجسٹر کھولیں اور دن کے لیے ابتدائی کیش درج کریں۔',
        'کیش سیلز، کسٹمر کیش پیمنٹس اور کوئی دستی کیش اِن رجسٹر میں شامل ہوتے ہیں۔',
        'سپلائر کیش پیمنٹس، کیش اخراجات، کسٹمر ریفنڈز اور کوئی دستی کیش آؤٹ اسے کم کرتے ہیں۔',
        'دن کے اختتام پر ڈیلی کلوزنگز کھولیں اور اصل گنی گئی کیش درج کریں۔',
        'سسٹم متوقع اختتامی کیش کا اصل اختتامی کیش سے موازنہ کر کے فرق دکھاتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'Opening Cash + Cash Sales + Customer Cash Payments + Manual Cash In − Supplier Cash Payments − Cash Expenses − Customer Refunds − Manual Cash Out = Expected Closing Cash, compared against the Actual Closing Cash you count and enter.',
      ur: 'ابتدائی کیش + کیش سیلز + کسٹمر کیش وصولی + دستی کیش اِن − سپلائر کیش ادائیگی − کیش اخراجات − کسٹمر ریفنڈ − دستی کیش آؤٹ = متوقع اختتامی کیش، جس کا موازنہ آپ کی گنی اور درج کردہ اصل اختتامی کیش سے کیا جاتا ہے۔',
    },
    relatedModules: ['sales', 'purchases', 'expenses'],
    importantNotes: {
      en: ['A mismatch between expected and actual cash is flagged automatically in Notifications.'],
      ur: ['متوقع اور اصل کیش میں فرق خودکار طور پر اطلاعات میں دکھایا جاتا ہے۔'],
    },
  },
  {
    key: 'bankManagement',
    icon: 'fas fa-university',
    route: '/shop-management/bank-accounts',
    requiredPolicy: 'ShopManagement.BankAccounts',
    name: { en: 'Bank Management', ur: 'بینک مینجمنٹ' },
    shortDescription: {
      en: 'Manage bank accounts, deposits, withdrawals, transfers, and balances.',
      ur: 'بینک اکاؤنٹس، جمع، نکاسی، ٹرانسفر اور بیلنس کا انتظام کریں۔',
    },
    purpose: {
      en: 'Keeps track of the shop\'s bank accounts and every transaction that moves money in or out of them.',
      ur: 'دکان کے بینک اکاؤنٹس اور ان میں آنے جانے والے ہر لین دین کا ریکارڈ رکھتا ہے۔',
    },
    whenToUse: {
      en: 'Whenever the shop deposits, withdraws, or transfers money through a bank account, or is paid/pays via bank.',
      ur: 'جب بھی دکان بینک اکاؤنٹ کے ذریعے رقم جمع، نکالے یا ٹرانسفر کرے، یا بینک کے ذریعے ادائیگی وصول/ادا کرے۔',
    },
    requiredInformation: {
      en: ['Bank account name and opening balance', 'Transaction amount and type (deposit, withdrawal, transfer)', 'Target account, for a transfer'],
      ur: ['بینک اکاؤنٹ کا نام اور ابتدائی بیلنس', 'لین دین کی رقم اور قسم (جمع، نکاسی، ٹرانسفر)', 'ٹرانسفر کی صورت میں منزل اکاؤنٹ'],
    },
    workflow: {
      en: [
        'Open Bank Management and add a Bank Account with its opening balance.',
        'Record Bank Transactions for deposits and withdrawals.',
        'Use Bank Transfers to move money between two of the shop\'s own accounts.',
        'Each account\'s balance updates automatically with every transaction.',
      ],
      ur: [
        'بینک مینجمنٹ کھولیں اور ابتدائی بیلنس کے ساتھ بینک اکاؤنٹ شامل کریں۔',
        'جمع اور نکاسی کے لیے بینک ٹرانزیکشنز درج کریں۔',
        'دکان کے اپنے دو اکاؤنٹس کے درمیان رقم منتقل کرنے کے لیے بینک ٹرانسفرز استعمال کریں۔',
        'ہر لین دین کے ساتھ اکاؤنٹ کا بیلنس خودکار طور پر اپڈیٹ ہوتا ہے۔',
      ],
    },
    whatHappensNext: {
      en: 'Bank payments/receipts recorded through Customer Payments, Supplier Payments, or Expenses also flow into the relevant bank account\'s balance.',
      ur: 'کسٹمر پیمنٹس، سپلائر پیمنٹس یا اخراجات کے ذریعے درج شدہ بینک ادائیگیاں/وصولیاں بھی متعلقہ بینک اکاؤنٹ کے بیلنس میں شامل ہوتی ہیں۔',
    },
    relatedModules: ['cashManagement', 'sales', 'purchases', 'expenses'],
    importantNotes: {
      en: ['A large unexpected drop in a bank balance is flagged automatically in Notifications.'],
      ur: ['بینک بیلنس میں غیرمتوقع بڑی کمی خودکار طور پر اطلاعات میں دکھائی جاتی ہے۔'],
    },
  },
  {
    key: 'reports',
    icon: 'fas fa-chart-pie',
    route: '/shop-management/reports',
    requiredPolicy: 'ShopManagement.Reports',
    name: { en: 'Reports', ur: 'رپورٹس' },
    shortDescription: {
      en: 'Sales, purchase, expense, stock, and ledger reports for a chosen date range.',
      ur: 'منتخب کردہ تاریخ کے لیے فروخت، خریداری، اخراجات، اسٹاک اور لیجر کی رپورٹس۔',
    },
    purpose: {
      en: 'A central place to review historical activity across the shop, filtered by date and other criteria.',
      ur: 'دکان کی سابقہ سرگرمیوں کا جائزہ لینے کی مرکزی جگہ، تاریخ اور دیگر معیار کے مطابق فلٹر شدہ۔',
    },
    whenToUse: {
      en: 'Whenever you need to review or export historical sales, purchases, expenses, or stock movement for a period.',
      ur: 'جب بھی کسی مدت کی فروخت، خریداری، اخراجات یا اسٹاک کی نقل و حرکت کا جائزہ یا ایکسپورٹ درکار ہو۔',
    },
    requiredInformation: { en: ['Date range and any report-specific filters'], ur: ['تاریخ کی حد اور رپورٹ سے متعلق مخصوص فلٹرز'] },
    workflow: {
      en: [
        'Open Reports.',
        'Choose the report and set the date range and filters.',
        'Review the results on screen, or use Export/Print where available.',
      ],
      ur: [
        'رپورٹس ماڈیول کھولیں۔',
        'رپورٹ منتخب کریں اور تاریخ کی حد اور فلٹرز سیٹ کریں۔',
        'نتائج اسکرین پر دیکھیں، یا دستیاب ہونے پر ایکسپورٹ/پرنٹ استعمال کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'Reports are read-only; they never change data in other modules.',
      ur: 'رپورٹس صرف دیکھنے کے لیے ہیں؛ یہ دیگر ماڈیولز کا ڈیٹا کبھی تبدیل نہیں کرتیں۔',
    },
    relatedModules: ['sales', 'purchases', 'expenses', 'inventory', 'profitLoss'],
    importantNotes: {
      en: ['Profit & Loss has its own dedicated page - see the Profit & Loss guide entry.'],
      ur: ['منافع و نقصان کا اپنا مخصوص صفحہ ہے - "منافع و نقصان" کی گائیڈ اندراج دیکھیں۔'],
    },
  },
  {
    key: 'profitLoss',
    icon: 'fas fa-balance-scale',
    route: '/shop-management/reports/profit-loss',
    requiredPolicy: 'ShopManagement.ProfitLoss.View',
    name: { en: 'Profit & Loss', ur: 'منافع و نقصان' },
    shortDescription: {
      en: 'A summary statement of sales, cost of goods sold, expenses, and the resulting profit or loss.',
      ur: 'فروخت، فروخت شدہ سامان کی لاگت، اخراجات اور نتیجتاً منافع یا نقصان کا خلاصہ بیان۔',
    },
    purpose: {
      en: 'Answers "did the shop make money?" for a chosen period, calculated automatically from posted sales, cost of goods sold, and posted expenses.',
      ur: 'منتخب کردہ مدت کے لیے "کیا دکان نے منافع کمایا؟" کا جواب دیتا ہے، جو مکمل شدہ فروخت، فروخت شدہ سامان کی لاگت اور پوسٹ شدہ اخراجات سے خودکار طور پر شمار ہوتا ہے۔',
    },
    whenToUse: {
      en: 'At the end of the day, week, or month to see whether the shop was profitable.',
      ur: 'دن، ہفتے یا مہینے کے اختتام پر یہ دیکھنے کے لیے کہ آیا دکان نے منافع کمایا۔',
    },
    requiredInformation: { en: ['Date range (defaults to the current month)'], ur: ['تاریخ کی حد (بطور ڈیفالٹ موجودہ مہینہ)'] },
    workflow: {
      en: [
        'Open Profit & Loss.',
        'Choose a date range, or use the current-month default.',
        'Review the four totals: Total Sales, Cost of Goods Sold, Total Expenses, and Net Profit/Loss.',
      ],
      ur: [
        'منافع و نقصان کھولیں۔',
        'تاریخ کی حد منتخب کریں، یا موجودہ مہینے کی ڈیفالٹ استعمال کریں۔',
        'چار مجموعی اعداد کا جائزہ لیں: کل فروخت، فروخت شدہ سامان کی لاگت، کل اخراجات اور خالص منافع/نقصان۔',
      ],
    },
    whatHappensNext: {
      en: 'Total Sales − Cost of Goods Sold = Gross Profit. Gross Profit − Total Expenses = Net Profit or Net Loss.',
      ur: 'کل فروخت − فروخت شدہ سامان کی لاگت = مجموعی منافع۔ مجموعی منافع − کل اخراجات = خالص منافع یا خالص نقصان۔',
    },
    relatedModules: ['sales', 'purchases', 'expenses', 'reports'],
    importantNotes: {
      en: ['This report is read-only and never creates or changes any sale, purchase, or expense record.', 'Only Completed sales and Posted expenses are counted.'],
      ur: ['یہ رپورٹ صرف دیکھنے کے لیے ہے اور کبھی کوئی فروخت، خریداری یا خرچے کا ریکارڈ نہیں بناتی یا بدلتی۔', 'صرف مکمل شدہ فروخت اور پوسٹ شدہ اخراجات شمار کیے جاتے ہیں۔'],
    },
  },
  {
    key: 'notifications',
    icon: 'fas fa-bell',
    route: '/shop-management/notifications',
    requiredPolicy: 'ShopManagement.Notifications.View',
    name: { en: 'Notifications', ur: 'اطلاعات' },
    shortDescription: {
      en: 'Automatic alerts for low stock, near-expiry batches, overdue balances, cash differences, and more.',
      ur: 'کم اسٹاک، قریب المیعاد بیچز، واجب الادا بیلنس، کیش کے فرق اور دیگر کے لیے خودکار اطلاعات۔',
    },
    purpose: {
      en: 'Surfaces things that need attention across the shop without having to check every module separately.',
      ur: 'ہر ماڈیول الگ سے چیک کیے بغیر توجہ طلب معاملات سامنے لاتا ہے۔',
    },
    whenToUse: {
      en: 'Check regularly, or whenever the bell icon shows unread alerts.',
      ur: 'باقاعدگی سے چیک کریں، یا جب بھی گھنٹی کا آئیکن غیر پڑھی گئی اطلاعات دکھائے۔',
    },
    requiredInformation: { en: [], ur: [] },
    workflow: {
      en: [
        'Open Notifications, or click the bell icon in the top bar.',
        'Review alerts: low stock, expired/near-expiry batches, overdue customer or supplier balances, a cash-register difference, or a profit/loss warning.',
        'Click a notification to open the related record.',
        'Mark it as read.',
      ],
      ur: [
        'اطلاعات کھولیں، یا ٹاپ بار میں گھنٹی کے آئیکن پر کلک کریں۔',
        'اطلاعات کا جائزہ لیں: کم اسٹاک، میعاد ختم/قریب المیعاد بیچز، کسٹمر یا سپلائر کا واجب الادا بیلنس، کیش رجسٹر کا فرق، یا منافع/نقصان کی وارننگ۔',
        'متعلقہ ریکارڈ کھولنے کے لیے اطلاع پر کلک کریں۔',
        'اسے پڑھا ہوا نشان زد کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'Notifications are generated automatically by the system on a schedule - they are informational and don\'t require any action to keep working.',
      ur: 'اطلاعات سسٹم کے ذریعے شیڈول کے مطابق خودکار طور پر بنتی ہیں - یہ صرف معلوماتی ہیں اور کام جاری رکھنے کے لیے کسی اقدام کی ضرورت نہیں۔',
    },
    relatedModules: ['inventory', 'customers', 'suppliers', 'cashManagement', 'profitLoss'],
    importantNotes: {
      en: ['Notification types include: low stock, out of stock, near-expiry/expired batches, customer overdue balance, supplier overdue balance, cash-register closing difference, and a profit/loss warning.'],
      ur: ['اطلاعات کی اقسام: کم اسٹاک، ختم شدہ اسٹاک، قریب المیعاد/میعاد ختم بیچز، کسٹمر کا واجب الادا بیلنس، سپلائر کا واجب الادا بیلنس، کیش رجسٹر کلوزنگ کا فرق، اور منافع/نقصان کی وارننگ۔'],
    },
  },
  {
    key: 'aiAssistant',
    icon: 'fas fa-robot',
    route: '/shop-management/ai-assistant',
    requiredPolicy: 'ShopManagement.AiAssistant',
    name: { en: 'AI Assistant', ur: 'اے آئی اسسٹنٹ' },
    shortDescription: {
      en: 'Ask questions in English or Urdu, look up records, and create supported records through text or voice.',
      ur: 'انگریزی یا اردو میں سوالات پوچھیں، ریکارڈز تلاش کریں، اور متن یا آواز کے ذریعے معاون ریکارڈز بنائیں۔',
    },
    purpose: {
      en: 'A conversational way to get quick answers (e.g. today\'s sales, stock of a product, a customer\'s balance) and to create simple records without navigating through the full forms.',
      ur: 'فوری جوابات حاصل کرنے کا ایک بات چیت پر مبنی طریقہ (مثلاً آج کی فروخت، کسی پروڈکٹ کا اسٹاک، کسٹمر کا بیلنس)، اور مکمل فارمز میں جائے بغیر آسان ریکارڈز بنانا۔',
    },
    whenToUse: {
      en: 'When it\'s faster to type or say a question than to navigate to the right module and filter.',
      ur: 'جب سوال ٹائپ یا بول کر پوچھنا صحیح ماڈیول تک جا کر فلٹر کرنے سے تیز ہو۔',
    },
    requiredInformation: {
      en: ['A question or instruction, typed or spoken', 'The details a specific action needs (e.g. customer name, product, amount) - the assistant will ask if something is missing'],
      ur: ['ایک سوال یا ہدایت، ٹائپ شدہ یا بولی گئی', 'کسی مخصوص اقدام کے لیے درکار تفصیلات (مثلاً کسٹمر کا نام، پروڈکٹ، رقم) - اگر کچھ کم ہو تو اسسٹنٹ خود پوچھے گا'],
    },
    workflow: {
      en: [
        'Open AI Assistant.',
        'Type a question, or use voice input.',
        'For a question (e.g. "what were today\'s sales?"), the assistant reads the relevant data and answers directly.',
        'For a request to create a record (e.g. a new customer), the assistant collects the required fields and shows a preview.',
        'Confirm the preview before anything is actually created - nothing is saved silently.',
      ],
      ur: [
        'اے آئی اسسٹنٹ کھولیں۔',
        'سوال ٹائپ کریں، یا آواز کے ذریعے بولیں۔',
        'کسی سوال کے لیے (مثلاً "آج کی فروخت کتنی ہوئی؟")، اسسٹنٹ متعلقہ ڈیٹا پڑھ کر براہ راست جواب دیتا ہے۔',
        'کوئی ریکارڈ بنانے کی درخواست کے لیے (مثلاً نیا کسٹمر)، اسسٹنٹ درکار معلومات جمع کر کے پیش نظارہ دکھاتا ہے۔',
        'کچھ بھی حقیقتاً بننے سے پہلے پیش نظارے کی تصدیق کریں - کچھ بھی خاموشی سے محفوظ نہیں ہوتا۔',
      ],
    },
    whatHappensNext: {
      en: 'Confirmed write actions create a real record in the relevant module (e.g. Customers, Products) exactly as previewed.',
      ur: 'تصدیق شدہ اقدامات متعلقہ ماڈیول میں (مثلاً کسٹمرز، پروڈکٹس) بالکل پیش نظارے کے مطابق اصل ریکارڈ بناتے ہیں۔',
    },
    relatedModules: ['sales', 'products', 'customers', 'suppliers', 'expenses'],
    importantNotes: {
      en: ['The assistant always asks for confirmation before creating or changing anything - it never silently creates or deletes records.', 'Only a limited set of record types can currently be created this way; more complex records may only be looked up, not created.'],
      ur: ['اسسٹنٹ کچھ بھی بنانے یا بدلنے سے پہلے ہمیشہ تصدیق طلب کرتا ہے - یہ خاموشی سے کبھی کوئی ریکارڈ نہیں بناتا یا مٹاتا۔', 'فی الحال صرف محدود اقسام کے ریکارڈز اس طریقے سے بنائے جا سکتے ہیں؛ زیادہ پیچیدہ ریکارڈز صرف تلاش کیے جا سکتے ہیں، بنائے نہیں جا سکتے۔'],
    },
  },
  {
    key: 'administration',
    icon: 'fas fa-user-shield',
    route: '/identity/users',
    name: { en: 'Administration', ur: 'انتظامیہ' },
    shortDescription: {
      en: 'Manage user accounts, roles, and permissions for who can access what.',
      ur: 'یوزر اکاؤنٹس، رولز اور کون کیا استعمال کر سکتا ہے اس کی اجازتیں منظم کریں۔',
    },
    purpose: {
      en: 'Controls who can log in and what each person is allowed to see or do across every module above.',
      ur: 'یہ کنٹرول کرتا ہے کہ کون لاگ اِن کر سکتا ہے اور ہر شخص کو مذکورہ بالا ہر ماڈیول میں کیا دیکھنے یا کرنے کی اجازت ہے۔',
    },
    whenToUse: {
      en: 'When adding a new staff member, changing what someone is allowed to do, or reviewing existing roles.',
      ur: 'نئے عملے کے فرد کو شامل کرتے وقت، کسی کی اجازتیں تبدیل کرتے وقت، یا موجودہ رولز کا جائزہ لیتے وقت۔',
    },
    requiredInformation: {
      en: ['Username, email, and password for a new user', 'The role(s) to assign, and which permissions that role grants'],
      ur: ['نئے یوزر کے لیے یوزرنیم، ای میل اور پاس ورڈ', 'تفویض کیے جانے والے رول(ز)، اور اس رول کی اجازتیں'],
    },
    workflow: {
      en: [
        'Open Administration → Users to add or edit a user account.',
        'Open Administration → Roles to create a role and choose which permissions it grants.',
        'Assign the appropriate role to each user.',
      ],
      ur: [
        'یوزر اکاؤنٹ شامل یا ترمیم کرنے کے لیے انتظامیہ → یوزرز کھولیں۔',
        'رول بنانے اور اس کی اجازتیں منتخب کرنے کے لیے انتظامیہ → رولز کھولیں۔',
        'ہر یوزر کو مناسب رول تفویض کریں۔',
      ],
    },
    whatHappensNext: {
      en: 'A user only sees the modules and menu items their role has permission for - everything else stays hidden, including in this guide.',
      ur: 'یوزر صرف وہی ماڈیولز اور مینو آئٹمز دیکھتا ہے جن کی اس کے رول کو اجازت ہو - باقی سب چھپا رہتا ہے، بشمول اس گائیڈ میں۔',
    },
    relatedModules: [],
    importantNotes: {
      en: ['This guide only lists the modules the current user is actually permitted to access.'],
      ur: ['یہ گائیڈ صرف وہی ماڈیولز دکھاتی ہے جن تک موجودہ یوزر کو رسائی کی اجازت ہے۔'],
    },
  },
];
