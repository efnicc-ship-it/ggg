# TeknikServis ERP — Claude Devam Kılavuzu

**Branch:** `claude/continue-previous-work-caTJj`
**Son güncelleme:** 2026-04-08

---

## Projeye Hızlı Giriş

Bu belgeyi okuyan Claude: TeknikServis ERP projesine devam ediyorsun.
Phase durumunu `docs/PROGRESS.md`'den oku. Oradan devam et.

---

## Projenin Amacı

Tek sistemde:
- Telefon/tablet/notebook **teknik servis** yönetimi
- Cihaz **alış/satış** + envanter
- **Aksesuar** (3 kategori) satış + stok
- **Tedarikçi** + parça sipariş yönetimi
- **Bayi portali**
- KVKK uyumlu **müşteri yönetimi**
- **Finans** modülü
- **Ensar AI** kural motoru
- **Multi-tenant SaaS** mimarisi

---

## Tech Stack

| Katman | Teknoloji | Not |
|---|---|---|
| Framework | C# ASP.NET Core 10 MVC + ayrı API projesi | MVC: Cookie auth / API: JWT |
| Database | MSSQL + Entity Framework Core 10 (Code-First) | |
| UI Tema | Metronic Tailwind Demo6 + Google Stitch UI | Stitch öncelikli |
| Responsive | Tailwind breakpoints (sm/md/lg/xl) | Mobil-first |
| Real-time | SignalR | Ensar AI uyarıları, canlı bildirim |
| Background Jobs | Hangfire + Hangfire.SqlServer | |
| PDF | QuestPDF | Sözleşmeler, raporlar |
| QR Kod | QRCoder | Etiketler, ürün sayfaları |
| Termal Etiket | ZPL (raw TCP socket) | Zebra yazıcı |
| Şifreleme | AES-256-GCM (System.Security.Cryptography) | KVKK |
| SMS | Yapılandırılabilir (NetGSM/Twilio HTTP) | Admin panelden provider seç |
| WhatsApp | Yapılandırılabilir (WhatsApp Business API) | |
| E-posta | FluentEmail + SMTP | |
| Barcode Okuma | ZXing.Net | Stok, IMEI tarama |
| Online Ödeme | iyzico veya PayTR SDK | Ödeme linki → SMS/WA |
| Loglama | Serilog + rolling file + Seq (opsiyonel) | |
| Mimari | Clean Architecture (CQRS + MediatR + FluentValidation) | |
| Auth (MVC) | ASP.NET Core Identity + Cookie | |
| Auth (API) | JWT Bearer + Refresh Token | |
| Doğrulama | FluentValidation | |
| Mapping | AutoMapper | |
| Multi-tenant | EF Core Global Query Filter + TenantId | |

---

## Solution Yapısı

```
TeknikServis.sln
├── src/
│   ├── TeknikServis.Domain/         # Saf domain — dış bağımlılık yok
│   ├── TeknikServis.Application/    # CQRS: Commands, Queries, DTOs, Interfaces
│   ├── TeknikServis.Infrastructure/ # EF Core, Servisler, Hangfire, Şifreleme
│   ├── TeknikServis.Web/            # ASP.NET Core 10 MVC — Admin panel (Cookie auth)
│   └── TeknikServis.Api/            # Ayrı REST API — JWT auth (mobil/3. parti için)
└── tests/
    ├── TeknikServis.Domain.Tests/
    └── TeknikServis.Application.Tests/
```

Bağımlılık yönü:
```
Web  → Application → Domain
Api  → Application → Domain
Web  → Infrastructure → Application → Domain
Api  → Infrastructure → Application → Domain
```

---

## Domain Katmanı — Entity'ler (Tam Liste)

### Enums
```
TeknikServis.Domain/Enums/
├── ServiceStatus.cs        # Received→Diagnosing→AwaitingParts→InRepair→AwaitingApproval→Ready→Delivered→Cancelled→Unrepairable→ReturnedUnrepaired
├── PurchaseType.cs         # Normal | Damaged | Scrap
├── PaymentStatus.cs        # Unpaid | PartiallyPaid | Paid | Deferred | OnlinePaymentPending
├── CommunicationPref.cs    # SMS | WhatsApp | Email | All
├── NotificationChannel.cs  # SMS | WhatsApp | Email | InApp | Push
├── WarrantyStatus.cs       # Active | Expired | Voided
├── UserRole.cs             # SuperAdmin|TenantOwner|GeneralManager|RegionalManager|BranchManager|Technician|Reception|Dealer
├── DeviceCondition.cs      # New | SecondHand | Damaged | Scrap
├── InventorySource.cs      # Supplier | CustomerPurchase | ScrapHarvest
├── InventoryStatus.cs      # Available | Reserved | Sold | Transferred | Scrapped | UnderRepair
├── ModuleName.cs           # Service | Purchase | Sale | Stock | Finance | Branch | Dealer | EnsarAI | Api
├── LicenseType.cs          # Monthly | HalfYearly | Yearly | Unlimited | Trial
├── TransferStatus.cs       # Pending | InTransit | Received | Rejected
├── PartOrderStatus.cs      # Ordered | PartiallyReceived | FullyReceived | Returned | Cancelled
└── AlertSeverity.cs        # Info | Warning | Critical | Emergency
```

### Entity Yapısı
```
TeknikServis.Domain/Entities/
├── Common/
│   ├── BaseEntity.cs              # Id, CreatedAt, UpdatedAt, TenantId
│   ├── IAuditableEntity.cs        # CreatedBy, UpdatedBy
│   └── ISoftDelete.cs             # IsDeleted, DeletedAt, DeletedBy
├── Tenant/
│   ├── Tenant.cs
│   ├── TenantModule.cs
│   └── TenantSettings.cs
├── Identity/
│   ├── AppUser.cs
│   ├── AppRole.cs
│   └── UserPermission.cs
├── Branch/
│   ├── Branch.cs
│   ├── BranchTransfer.cs
│   └── ServiceBranchTransfer.cs
├── Customer/
│   ├── Customer.cs                # KVKK: NationalIdEncrypted(AES), IdCardPhotoPath(encrypted)
│   ├── CustomerRating.cs
│   ├── CustomerDocument.cs
│   └── CustomerSurveyResponse.cs
├── Device/                        # ORTAK KATALOG HAVUZU
│   ├── DeviceType.cs              # Telefon | Tablet | Notebook
│   ├── DeviceBrand.cs             # DeviceTypeId FK — type seçilmeden brand yok
│   ├── DeviceModel.cs             # BrandId FK — brand seçilmeden model yok
│   ├── DeviceModelVariant.cs      # 128GB/256GB — her varyantın kendi ModelCode'u
│   ├── DeviceCatalog.cs
│   ├── DeviceInventory.cs         # Fiziksel stok: IMEI, SN, durum, kaynak, fiyat
│   └── IntakeChecklist.cs
├── Service/
│   ├── ServiceRecord.cs
│   ├── ServiceStatusHistory.cs
│   ├── ServiceMedia.cs            # 2 yıl otomatik silinir
│   ├── ServiceNote.cs
│   ├── ServicePart.cs
│   ├── ServicePayment.cs
│   ├── ServiceTimeLog.cs
│   ├── LiveStreamSchedule.cs
│   └── PriceApprovalLog.cs
├── CommonFault/
│   ├── CommonFaultType.cs
│   └── CommonActionType.cs
├── PriceList/
│   └── RepairPriceList.cs
├── Purchase/
│   ├── PurchaseRecord.cs
│   └── PurchaseDocument.cs
├── Sale/
│   ├── SaleRecord.cs
│   ├── SaleItem.cs
│   ├── WarrantyRecord.cs
│   ├── DiscountRecord.cs
│   └── OnlinePaymentRecord.cs
├── Campaign/
│   └── Campaign.cs
├── Appointment/
│   └── AppointmentRecord.cs
├── Dealer/
│   ├── Dealer.cs
│   └── DealerDevice.cs
├── Supplier/
│   ├── Supplier.cs
│   ├── PartOrder.cs
│   ├── PartOrderItem.cs
│   └── SupplierPartPrice.cs
├── Stock/
│   ├── Part.cs
│   ├── PartCompatibility.cs
│   ├── DeviceAccessory.cs
│   ├── DeviceCompanion.cs
│   ├── UniversalAccessory.cs
│   ├── StockItem.cs
│   └── StockMovement.cs
├── Finance/
│   ├── Account.cs
│   ├── AccountTransaction.cs
│   ├── CustomerReceivable.cs
│   ├── DealerReceivable.cs
│   └── SupplierPayable.cs
├── Notification/
│   ├── NotificationTemplate.cs
│   └── NotificationLog.cs
├── EnsarAI/
│   ├── AiRule.cs
│   ├── AiAlert.cs
│   └── EscalationPolicy.cs
├── Blacklist/
│   ├── BlacklistedCustomer.cs
│   └── BlacklistedImei.cs
├── Audit/
│   └── AuditLog.cs
└── InternalChat/
    ├── Conversation.cs
    ├── ConversationMember.cs
    ├── Message.cs
    └── MessageAttachment.cs
```

---

## Cihaz Kataloğu Hiyerarşi Kuralı

```
DeviceType → DeviceBrand (TypeId FK) → DeviceModel (BrandId FK) → DeviceModelVariant (ModelId FK)
```

UI'da:
1. Cihaz Türü seçilmeden Brand dropdown **disabled**
2. Brand seçilmeden Model dropdown **disabled**
3. Model seçilince Variant listesi çıkar (HasVariants=true ise)
4. Model Kodu readonly — seçime göre otomatik dolar

---

## Key Entity Alanları

### Customer.cs
```
FirstName, LastName, Phone (unique), Email, Address
CommunicationPreference (SMS | WhatsApp)
NationalIdEncrypted       // AES-256 cipher, base64 stored
IdCardPhotoPath           // encrypted file yolu
IsBlacklisted, BlacklistReason
Rating (0-5), RatingCount
IsDealer, DealerId?
```

### ServiceRecord.cs
```
RecordNumber              // SRV-2024-001234 (unique, indexed)
CustomerId, DealerId?
DeviceModelId
Imei1?, Imei2?, SerialNumber?
DeviceColor?, DevicePassword? (encrypted)
FaultDescription
Status (ServiceStatus enum)
ApprovalToken (GUID, public query sayfası için)
PriceApproved?, QuotedPrice?, FinalPrice?
PaymentStatus, LabelPrinted
AssignedTechnicianId?
DeliveredAt?
```

### PurchaseRecord.cs
```
RecordNumber              // ALI-2024-001234
CustomerId, DeviceModelId
PurchaseType (Normal|Damaged|Scrap)
Imei1, Imei2?, SerialNumber?, Ram?, Storage?, Color?
PurchasePrice
ImeiCheckPdfPath?
ContractPdfPath?, LabelPrinted
LinkedServiceRecordId?    // Hasarlı alış → servis bağlantısı
PartsHarvested
```

### SaleRecord.cs
```
RecordNumber              // SAT-2024-001234
CustomerId?
PurchaseRecordId?
IsAccessorySaleOnly
TotalAmount
ContractPdfPath?
WarrantyDays?, WarrantyExpiresAt?
PaymentStatus
```

---

## Application Katmanı (CQRS)

```
TeknikServis.Application/
├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs
│   │   ├── IEncryptionService.cs
│   │   ├── IFileStorageService.cs
│   │   ├── ISmsService.cs
│   │   ├── IWhatsAppService.cs
│   │   ├── IPdfService.cs
│   │   ├── ILabelPrintService.cs
│   │   └── ICurrentUserService.cs
│   ├── Behaviors/
│   │   ├── ValidationBehavior.cs
│   │   └── LoggingBehavior.cs
│   └── Mappings/MappingProfile.cs
├── Features/
│   ├── Customers/
│   ├── ServiceRecords/
│   ├── Purchases/
│   ├── Sales/
│   ├── Dealers/
│   ├── Suppliers/
│   ├── Stock/
│   └── Notifications/
└── BackgroundJobs/
    ├── MediaCleanupJob.cs
    ├── SmartReminderJob.cs
    ├── LowStockAlertJob.cs
    └── DatabaseBackupJob.cs
```

---

## Şifreleme Stratejisi (KVKK)

| Veri | Strateji |
|---|---|
| TC Kimlik No | AES-256 encrypt → Base64 → DB string kolonu |
| Kimlik fotoğrafı | AES-256 file encrypt → /uploads/secure/ |
| Cihaz şifresi (PIN) | AES-256 encrypt → DB string kolonu |
| Cihaz resimleri/videoları | Şifresiz → /uploads/media/ → 2 yıl sonra Hangfire siler |
| e-devlet IMEI PDF | Şifresiz → /uploads/documents/ |
| Sözleşme PDF | Şifresiz → /uploads/contracts/ |

AES Key: appsettings.json → **environment variable override** (production'da env var)

---

## Web Katmanı — URL Yapısı

```
/giris /cikis /pano /profil
/servis /servis/yeni /servis/{no} /servis/{no}/duzenle
/musteriler /musteriler/yeni /musteriler/{id}
/alis /alis/yeni /alis/{no}
/satis /satis/yeni /satis/{no}
/envanter /envanter/sifir /envanter/ikinci-el /envanter/{id}
/stok /stok/parcalar /stok/aksesuarlar /stok/hareketler
/tedarikciler /siparisler
/bayiler /subeler /transferler
/finans /finans/alacaklar /finans/borclar
/raporlar/servis /raporlar/alis-satis /raporlar/stok /raporlar/finans
/mesajlar /mesajlar/{konusmaId}
/ayarlar /ayarlar/kullanicilar /ayarlar/ai-kurallari
/bayi/giris /bayi/pano /bayi/cihazlarim /bayi/cari
/portal/giris /portal/pano /portal/cihazlarim
/c/{token}     → [AllowAnonymous] cihaz durum
/u/{slug}      → [AllowAnonymous] ürün vitrin
/randevu       → [AllowAnonymous]
/sa/tenantlar  → SuperAdmin
```

---

## UI Kuralları — KESİN SINIRLAR

1. **Metronic Tailwind Demo6** — wwwroot altında statik dosyalar, tüm Razor view'lar Demo6 CSS sınıflarını kullanır
2. **Google Stitch öncelikli** — Çakışma olursa Stitch kazanır
3. **Status göstergelerinde ikon KULLANILMAZ** — Sadece renkli badge + metin
4. **Tailwind breakpoints** — sm/md/lg/xl tam responsive

---

## Rol Hiyerarşisi

```
SuperAdmin → TenantOwner → GeneralManager → RegionalManager → BranchManager → Technician/Reception → Dealer
```

Granüler izinler: `Permissions.Service.Create`, `Permissions.Finance.ViewReports`, vb. (100+ izin)

---

## Ensar AI — 12 Default Kural

| Durum | Aksiyon |
|---|---|
| Servis 24h hareketsiz | Teknisyene InternalChat |
| Servis 48h hareketsiz | BranchManager'a WA |
| Parça siparişi 4h girilmemiş | Teknisyene InternalChat |
| 5+ silme art arda | GM'e Email + InternalChat |
| Gece 22:00-07:00 giriş | SecurityLog + GM Email |
| Stok threshold altı | StockManager InternalChat |
| Bayi 30 gün ödeme yok | FinanceManager haftalık özet |
| Cihaz 60 gün envanterde | SalesManager öneri |
| Garantili servis geldi | Teknisyene Banner |
| Kara liste IMEI | Anında GM + log |
| IMEI kontrolsüz satın alma | Bloke + GM bildirim |
| Kara liste müşteri girişi | Anlık uyarı |

---

## Termal Etiket Şablonları (3 adet)

**1. Servis Etiketi:** QR(durum sayfası) + 9 nokta alan + Ad/Tel/Model/Servis No — Ücret YOK
**2. Aksesuar Etiketi:** QR(ürün sayfası) + Ürün/Uyumlu model — Ücret YOK
**3. Cihaz Envanter Etiketi:** QR(ürün sayfası) + Model/Pil/Hafıza/Garanti — Ücret YOK

---

## QR Kod Davranışı

```
/u/{slug} okunuyor:
  ├── Giriş yapılmamışsa → Ürün tanıtım sayfası (public)
  └── Giriş yapılmış + Permissions.Stock.QuickSell izni → "Stoktan Düş" popup

/c/{token} okunuyor:
  ├── Herkese açık → Cihaz durum sayfası
  └── Admin girişi varsa → + Düzenle butonu
```

---

## Stok Uyarısı (Servis Kaydında)

- Yeşil: "Stokta X adet mevcut"
- Sarı: "Kritik stok, X adet kaldı"
- Kırmızı: "Stokta yok"
- **Her durumda sipariş verilebilir — uyarı bloke etmez**

---

## Raporlama Zaman Filtreleri (Tüm Raporlarda)

`Bugün | Dün | Son 3 Gün | Son 1 Hafta | Son 1 Ay | Son 1 Yıl | Bu Hafta | Bu Ay | Bu Yıl | Özel Aralık`

Export: **Excel (.xlsx) + PDF** tüm raporlarda.

---

## Uygulama Aşamaları

### Phase 1 — Temel Altyapı ✅ TAMAMLANDI
- Solution scaffold (5 proje)
- Multi-tenant altyapısı
- Domain katmanı: tüm entity'ler + enum'lar
- Infrastructure: DbContext + EF configurations + migration
- AES-256 encryption service
- Identity: AppUser + AppRole + UserPermission
- SignalR hub kurulumu
- API projesi: JWT auth, /health endpoint
- Hangfire + seeder (12 AI kuralı)

### Phase 2 — Müşteri + Servis (Çekirdek) ⏳ SIRADAKI
- Customer CRUD + KVKK şifreleme + popup partial
- Device Catalog (marka/model ortak havuz) + AJAX cascading
- CommonFault + CommonAction listeleri
- ServiceRecord CRUD + durum makinesi
- IMEI bazlı geçmiş + kara liste kontrolü
- Servis termal etiket (ZPL) + QR + 9 nokta alan
- SMS + WhatsApp + Email bildirim
- Public device status page `/c/{token}` [AllowAnonymous]
- Fiyat onay akışı
- Before/after fotoğraf karşılaştırma
- Teknisyen zaman takibi
- Standart fiyat listesi + maliyet-kar hesabı
- Servis kaydından hızlı parça sipariş paneli
- Otomatik stok düşümü (Ready durumunda)
- Garanti kontrolü

### Phase 3 — Alış/Satış + Aksesuar ⏳
- Purchase Module: Normal/Hasarlı/Hurda + intake checklist
- IMEI kontrol zorunluluğu + e-devlet PDF
- Dijital imza
- Alış sözleşmesi (QuestPDF)
- DeviceInventory (tam detay)
- Sale Module + garanti
- Satış sözleşmesi + online ödeme linki
- Aksesuar satışı (3 kategori)
- Public ürün vitrin sayfaları `/u/{slug}`
- Kampanya/indirim yönetimi
- Hurda → parça stoğa ekleme

### Phase 4 — Bayi + Tedarikçi + Finans ⏳
- Dealer portal
- Bayi servis talebi → admin bildirim
- Supplier management + parça uyumluluk matrisi
- PartOrder: WA sipariş + kısmi/tam teslim
- Finans modülü: hesap planı, alacak/borç, nakit akışı

### Phase 5 — Stok + Şubeler + Mesajlaşma + Arka Plan ⏳
- Stok/Inventory modülü (parça + 3x aksesuar)
- Barcode scanner (ZXing.Net)
- Şube yönetimi + şubeler arası transfer
- İç Mesajlaşma (SignalR: birebir + grup, dosya eki, hiyerarşik yetki)
- Hangfire background jobs (media cleanup, hatırlatıcılar, backup)
- Ensar AI kural motoru (tam implementasyon)
- Randevu sistemi

### Phase 6 — Raporlar + Analitik + SaaS ⏳
- Kapsamlı raporlama (tüm modüller + Excel + PDF)
- Super Admin paneli
- Abonelik + modül lisanslama
- WhatsApp chatbot (pasif sorgulama)
- REST API tam feature coverage

---

## Kritik Teknik Kararlar

- **Silme:** Onay popup + zorunlu sebep + AuditLog — "4-eyes" YOK
- **Stok düşümü:** Cihaz "Ready" durumuna geçince otomatik
- **IMEI kontrolsüz alım:** Sistem engellemez ama Ensar AI müdüre bildirir
- **Medya:** /uploads/media/{year}/ → 2 yıl sonra Hangfire siler
- **Kamera:** HTML5 `<input type="file" capture="environment">`
- **Bayi erişimi:** [Authorize(Roles="Dealer")] + DealerId filtresi
- **EF Global Filter:** TenantId + IsDeleted + BranchId otomatik — controllerlarda manuel WHERE yok

---

## Güvenlik Özeti

- PBKDF2 şifre hash (Identity default)
- AES-256-GCM hassas veri şifreleme
- HTTPS + TLS 1.3 zorunlu
- Security headers middleware (CSP, X-Frame-Options, HSTS...)
- Rate limiting: /api/ 100 req/min, /auth/login 5 req/min
- Dosya yükleme: MIME + magic bytes + uzantı whitelist
- Audit log: append-only, update/delete yasak
- AES key: environment variable (asla kodda/config'de)

---

## Devam Talimatı (Yeni Oturum)

```
TeknikServis ERP projesine devam ediyoruz.
- Repo: /home/user/ggg
- Branch: claude/continue-previous-work-caTJj
- Plan: CLAUDE.md (bu dosya)
- İlerleme: docs/PROGRESS.md

Phase 1 tamamlandı. Phase 2'den devam et.
```
