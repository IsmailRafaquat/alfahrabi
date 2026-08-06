import { AuthService } from '@abp/ng.core';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';

type HomeLang = 'en' | 'ur';

interface HomeComponentTile {
  icon: string;
  title: string;
  description: string;
}

interface HomeContent {
  heroEyebrow: string;
  heroTitle: string;
  heroDescription: string;
  loginBtn: string;
  talkToUsBtn: string;
  panelSalesTitle: string;
  panelSalesDesc: string;
  panelInventoryTitle: string;
  panelInventoryDesc: string;
  panelReportsTitle: string;
  panelReportsDesc: string;
  highlight1: string;
  highlight2: string;
  highlight3: string;
  highlight4: string;
  componentsTitle: string;
  componentsSubtitle: string;
  components: HomeComponentTile[];
  ctaTitle: string;
  ctaDescription: string;
  ctaLoginBtn: string;
  ctaWhatsAppBtn: string;
  trustSecure: string;
  trustFastSetup: string;
  trustSupport: string;
}

const HOME_CONTENT: Record<HomeLang, HomeContent> = {
  en: {
    heroEyebrow: 'Shop Management',
    heroTitle: 'Run your entire shop from one place',
    heroDescription:
      "Sales, purchases, inventory, customers, suppliers, expenses, cash & bank — one system to record every transaction, track stock in real time and see exactly where your money is going.",
    loginBtn: 'Login',
    talkToUsBtn: 'Talk to Us',
    panelSalesTitle: 'Sales & POS',
    panelSalesDesc: 'Invoice in seconds, track every payment',
    panelInventoryTitle: 'Live Inventory',
    panelInventoryDesc: 'Stock, batches and expiry, always up to date',
    panelReportsTitle: 'Financial Reports',
    panelReportsDesc: 'Profit & loss, receivables and payables',
    highlight1: 'Fast, guided workflows',
    highlight2: 'Role-based, secure access',
    highlight3: 'Real-time dashboards',
    highlight4: 'Dedicated support',
    componentsTitle: "What's Included",
    componentsSubtitle: "Every part of your shop's day-to-day operation, connected in a single system.",
    components: [
      { icon: 'fa-cash-register', title: 'Sales / POS', description: 'Fast point-of-sale billing, customer payments, ledgers and sale returns.' },
      { icon: 'fa-file-invoice-dollar', title: 'Purchases', description: 'Purchase orders, goods receipts, supplier payments and purchase returns.' },
      { icon: 'fa-boxes', title: 'Products', description: 'Product catalog, categories and units, all kept consistent across the shop.' },
      { icon: 'fa-warehouse', title: 'Inventory', description: 'Stock transactions, adjustments, stock counts and batch/expiry tracking.' },
      { icon: 'fa-users', title: 'Customers', description: 'Customer profiles, ledgers and outstanding receivables at a glance.' },
      { icon: 'fa-truck', title: 'Suppliers', description: 'Supplier profiles, ledgers and payables so nothing is missed.' },
      { icon: 'fa-receipt', title: 'Expenses', description: 'Record and categorize every business expense as it happens.' },
      { icon: 'fa-wallet', title: 'Cash Management', description: 'Cash register sessions, transactions and daily closings.' },
      { icon: 'fa-university', title: 'Bank Management', description: 'Bank accounts, transactions and transfers, reconciled with the shop.' },
      { icon: 'fa-chart-pie', title: 'Reports & Analytics', description: 'Sales, purchase, stock, tax and profit & loss reports on demand.' },
      { icon: 'fa-robot', title: 'AI Assistant', description: "Ask plain-language questions about your shop's data and get instant answers." },
      { icon: 'fa-cog', title: 'Settings', description: 'Shop profile, currency, tax and document preferences, all in one place.' },
    ],
    ctaTitle: 'Ready to manage your shop smarter?',
    ctaDescription: "Sign in to open your dashboard, or reach out and we'll help you get set up.",
    ctaLoginBtn: 'Login to Dashboard',
    ctaWhatsAppBtn: 'Talk on WhatsApp',
    trustSecure: 'Secure',
    trustFastSetup: 'Fast Setup',
    trustSupport: 'Support',
  },
  ur: {
    heroEyebrow: 'شاپ مینجمنٹ',
    heroTitle: 'اپنی پوری دکان ایک ہی جگہ سے چلائیں',
    heroDescription:
      'سیلز، خریداری، انوینٹری، کسٹمرز، سپلائرز، اخراجات، نقدی اور بینک — ایک ہی نظام میں ہر لین دین ریکارڈ کریں، اسٹاک کو حقیقی وقت میں ٹریک کریں اور دیکھیں کہ آپ کا پیسہ کہاں جا رہا ہے۔',
    loginBtn: 'لاگ ان',
    talkToUsBtn: 'ہم سے رابطہ کریں',
    panelSalesTitle: 'سیلز اور POS',
    panelSalesDesc: 'سیکنڈوں میں انوائس بنائیں، ہر ادائیگی ٹریک کریں',
    panelInventoryTitle: 'لائیو انوینٹری',
    panelInventoryDesc: 'اسٹاک، بیچز اور میعاد ختم ہونے کی تاریخ، ہمیشہ اپ ٹو ڈیٹ',
    panelReportsTitle: 'مالیاتی رپورٹس',
    panelReportsDesc: 'نفع و نقصان، وصولیاں اور واجبات',
    highlight1: 'تیز اور آسان ورک فلو',
    highlight2: 'کردار پر مبنی محفوظ رسائی',
    highlight3: 'حقیقی وقت کے ڈیش بورڈز',
    highlight4: 'مخصوص سپورٹ',
    componentsTitle: 'اس میں کیا شامل ہے',
    componentsSubtitle: 'آپ کی دکان کے روزمرہ کے ہر کام کو ایک ہی نظام میں جوڑا گیا ہے۔',
    components: [
      { icon: 'fa-cash-register', title: 'سیلز / POS', description: 'تیز POS بلنگ، کسٹمر ادائیگیاں، لیجرز اور سیل ریٹرن۔' },
      { icon: 'fa-file-invoice-dollar', title: 'خریداری', description: 'خریداری کے آرڈرز، سامان کی وصولی، سپلائر ادائیگیاں اور خریداری واپسی۔' },
      { icon: 'fa-boxes', title: 'پراڈکٹس', description: 'پراڈکٹ کیٹلاگ، اقسام اور یونٹس، پوری دکان میں یکساں۔' },
      { icon: 'fa-warehouse', title: 'انوینٹری', description: 'اسٹاک ٹرانزیکشنز، ایڈجسٹمنٹس، اسٹاک کاؤنٹس اور بیچ/معیاد ٹریکنگ۔' },
      { icon: 'fa-users', title: 'کسٹمرز', description: 'کسٹمر پروفائلز، لیجرز اور بقایا وصولیاں ایک نظر میں۔' },
      { icon: 'fa-truck', title: 'سپلائرز', description: 'سپلائر پروفائلز، لیجرز اور واجبات، کچھ بھی نہیں چھوٹے گا۔' },
      { icon: 'fa-receipt', title: 'اخراجات', description: 'ہر کاروباری خرچ کو فوری طور پر ریکارڈ اور درجہ بندی کریں۔' },
      { icon: 'fa-wallet', title: 'نقدی کا انتظام', description: 'کیش رجسٹر سیشنز، ٹرانزیکشنز اور روزانہ کلوزنگ۔' },
      { icon: 'fa-university', title: 'بینک کا انتظام', description: 'بینک اکاؤنٹس، ٹرانزیکشنز اور ٹرانسفرز، دکان کے ساتھ مطابقت شدہ۔' },
      { icon: 'fa-chart-pie', title: 'رپورٹس اور تجزیات', description: 'سیلز، خریداری، اسٹاک، ٹیکس اور نفع و نقصان کی رپورٹس حسبِ ضرورت۔' },
      { icon: 'fa-robot', title: 'AI اسسٹنٹ', description: 'اپنی دکان کے ڈیٹا کے بارے میں سادہ زبان میں سوالات پوچھیں اور فوری جواب حاصل کریں۔' },
      { icon: 'fa-cog', title: 'ترتیبات', description: 'دکان کی پروفائل، کرنسی، ٹیکس اور دستاویزی ترجیحات، ایک ہی جگہ پر۔' },
    ],
    ctaTitle: 'اپنی دکان کو بہتر طریقے سے چلانے کے لیے تیار ہیں؟',
    ctaDescription: 'اپنا ڈیش بورڈ کھولنے کے لیے لاگ ان کریں، یا رابطہ کریں اور ہم آپ کی مدد کریں گے۔',
    ctaLoginBtn: 'ڈیش بورڈ میں لاگ ان کریں',
    ctaWhatsAppBtn: 'واٹس ایپ پر بات کریں',
    trustSecure: 'محفوظ',
    trustFastSetup: 'تیز سیٹ اپ',
    trustSupport: 'سپورٹ',
  },
};

@Component({
  standalone: false,
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
})
export class HomeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  lang: HomeLang = 'en';

  get hasLoggedIn(): boolean {
    return this.authService.isAuthenticated;
  }

  get t(): HomeContent {
    return HOME_CONTENT[this.lang];
  }

  get dir(): 'ltr' | 'rtl' {
    return this.lang === 'ur' ? 'rtl' : 'ltr';
  }

  ngOnInit(): void {
    if (this.hasLoggedIn) {
      this.router.navigate(['/shop-management/dashboard'], { replaceUrl: true });
    }
  }

  setLang(lang: HomeLang): void {
    this.lang = lang;
  }

  login(): void {
    this.authService.navigateToLogin();
  }
}
