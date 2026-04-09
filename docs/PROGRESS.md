# TeknikServis ERP — İlerleme Takibi

Son güncelleme: 2026-04-09

---

## Phase 1 — Temel Altyapı ✅ TAMAMLANDI

- ✅ Solution scaffold (Domain, Application, Infrastructure, Web, Api)
- ✅ Multi-tenant altyapısı (Tenant, TenantModule, TenantId global query filter)
- ✅ Domain katmanı: 60+ entity + tüm enum'lar
- ✅ Infrastructure: ApplicationDbContext + EF configurations + InitialCreate migration
- ✅ AES-256-GCM encryption service + encrypted file storage
- ✅ Identity: AppUser + AppRole + UserPermission + rol hiyerarşisi
- ✅ Web: Program.cs (Cookie auth, SignalR, Hangfire, security headers, rate limiter)
- ✅ SignalR: NotificationHub + ChatHub
- ✅ API: Program.cs (JWT auth, CORS, rate limiter, OpenAPI)
- ✅ API: HealthController + AuthController (login → JWT multi-role claims)
- ✅ Infrastructure: ZplLabelService, QrCodeService, SmsService, WhatsAppService
- ✅ Hangfire + ApplicationDbSeeder (12 Ensar AI kuralı)
- ✅ .gitignore (bin/obj exclude)

---

## Phase 2 — Müşteri + Servis (Çekirdek) 🔄 DEVAM EDİYOR

- ✅ Customer CRUD (Index, Detail, Edit, CreatePopup AJAX)
- ✅ Customer popup partial (servis/satış formundan hızlı müşteri ekleme)
- ✅ Catalog AJAX endpoints: Type→Brand→Model→Variant cascading + customer/part arama
- ✅ Sale Create view + CreateSaleCommand handler (cihaz + aksesuar modları)
- ✅ Hangfire background jobs:
  - ✅ MediaCleanupJob (2 yıl sonra soft-delete + dosya silme, her gece 03:00)
  - ✅ LowStockAlertJob (threshold altı stok → AiAlert, her saat)
  - ✅ SmartReminderJob (hareketsiz servis + anket, saatlik/2 saatlik)
  - ✅ AiRuleEngineJob (teknisyen/parça/bayi/cihaz kuralları, kayıtlı)
- ✅ ServiceRecord.SurveySent alanı + migration (20260409070000)
- ✅ CurrentUserService (ICurrentUserService implementasyonu)
- ✅ NotificationService (INotificationService → SMS/WA dispatch)
- ✅ ServiceRecord CQRS: CreateServiceRecord, UpdateServiceStatus, AddServicePart
- ✅ ServiceRecord queries: GetServiceRecords, GetServiceRecordDetail
- ✅ ServiceController: tam CRUD + durum makinesi + bayi akışı
- ✅ Service views: Index, Create, Detail, PublicQuery, PublicApproved, DealerIncoming, DealerPool
- ✅ Device Catalog Admin CRUD (Type/Brand/Model/Variant + AJAX)
- ✅ DeviceInventoryController + Index/Detail views
- ✅ AiAlertController + Index view (resolve/resolve-all)
- ✅ AppointmentController + Index view
- ✅ ReportController + Index view (aylık KPI özeti)
- ✅ AdminController.Index + Admin hub view
- ✅ StockController.Index redirect
- ✅ Dashboard AI alert linki düzeltildi
- ⏳ CommonFault + CommonAction listeleri (admin CRUD)
- ⏳ IMEI kara liste kontrolü (servis açılırken alert)
- ⏳ IMEI bazlı geçmiş kontrolü + kara liste kontrolü
- ⏳ Termal etiket (ZPL) + QR + 9 nokta alan
- ⏳ SMS + WhatsApp + Email bildirim akışı
- ⏳ Public device status page `/c/{token}` [AllowAnonymous]
- ⏳ Fiyat onay akışı (müşteri onay sayfası)
- ⏳ Before/after fotoğraf karşılaştırma
- ⏳ Teknisyen zaman takibi (başlat/durdur)
- ⏳ Standart fiyat listesi + maliyet-kar hesabı
- ⏳ Servis kaydından hızlı parça sipariş paneli
- ⏳ Otomatik stok düşümü (Ready durumunda)
- ⏳ Garanti kontrolü (servis açılırken)

---

## Phase 3 — Alış/Satış + Aksesuar ⏳ BAŞLAMADI

- ⏳ Purchase Module: Normal/Hasarlı/Hurda + intake checklist
- ⏳ IMEI kontrol zorunluluğu + e-devlet PDF upload
- ⏳ Dijital imza (tablet)
- ⏳ Alış sözleşmesi (QuestPDF)
- ⏳ DeviceInventory (tam detay, kaynak, durum)
- ⏳ Sale Module + garanti tanımlama
- ⏳ Satış sözleşmesi + online ödeme linki (iyzico/PayTR)
- ⏳ Aksesuar satışı (3 kategori)
- ⏳ Public ürün vitrin sayfaları `/u/{slug}`
- ⏳ Kampanya/indirim yönetimi
- ⏳ Hurda → parça stoğa ekleme akışı

---

## Phase 4 — Bayi + Tedarikçi + Finans ⏳ BAŞLAMADI

- ⏳ Dealer portal (ayrı login, kısıtlı görünüm, cari hesap)
- ⏳ Bayi servis talebi → admin bildirim + WA
- ⏳ Supplier management + parça uyumluluk matrisi
- ⏳ PartOrder: WA sipariş + kısmi/tam teslim + iade
- ⏳ Tedarikçi fiyat geçmişi analizi
- ⏳ Finans modülü: hesap planı, alacak/borç, nakit akışı
- ⏳ Müşteri alacak takibi + otomatik hatırlatma
- ⏳ Şube bazlı finans raporları

---

## Phase 5 — Stok + Şubeler + Mesajlaşma + Arka Plan ⏳ BAŞLAMADI

- ⏳ Stok/Inventory modülü (parça + 3x aksesuar kategorisi)
- ⏳ Barcode scanner (ZXing.Net)
- ⏳ Şube yönetimi + şubeler arası transfer
- ⏳ İç Mesajlaşma (SignalR: birebir + grup, dosya eki, hiyerarşik yetki)
- ⏳ Hangfire background jobs (media cleanup, hatırlatıcılar, low-stock, backup)
- ⏳ Ensar AI kural motoru tam implementasyon
- ⏳ Sipariş → AI entegrasyonu (tedarikçiye WA, toggle)
- ⏳ Randevu sistemi

---

## Phase 6 — Raporlar + Analitik + SaaS ⏳ BAŞLAMADI

- ⏳ Kapsamlı raporlama (servis/alış-satış/stok/finans/müşteri/AI)
- ⏳ Excel (.xlsx) + PDF export tüm raporlarda
- ⏳ Super Admin paneli (tenant yönetimi, modül aktivasyon)
- ⏳ Abonelik yönetimi + modül lisanslama
- ⏳ WhatsApp chatbot (pasif sorgulama webhook)
- ⏳ REST API tam feature coverage
- ⏳ Otomatik yedekleme doğrulama + restore testi
