# TeknikServis ERP — İlerleme Takibi

> Son güncelleme: 2026-04-13
> Branch: `claude/erp-device-management-xSpLl`

---

## GENEL DURUM

```
Phase 1 — Altyapı      ████████████████████ %95 (migration + factory eksik değildi ✅)
Phase 2 — Servis Çekirdeği  ████████████░░░░░░░░ %60 (Create + Detail view eksik)
Phase 3 — Alış/Satış   ░░░░░░░░░░░░░░░░░░░░ %0
Phase 4 — Bayi/Tedarik ████████░░░░░░░░░░░░ %40 (admin yüzü tamam, portal yok)
Phase 5 — Stok/Şube    ██░░░░░░░░░░░░░░░░░░ %10
Phase 6 — Raporlar     ░░░░░░░░░░░░░░░░░░░░ %0
```

---

## PHASE 1 — TEMEL ALTYAPI ✅ TAMAMLANDI

### Domain
- ✅ BaseEntity (Id, TenantId: Guid, CreatedAt, UpdatedAt)
- ✅ IAuditableEntity (CreatedBy, UpdatedBy)
- ✅ ISoftDelete (IsDeleted, DeletedAt, DeletedBy)
- ✅ Tüm enum'lar (ServiceStatus 17 durum, PurchaseType, PaymentStatus, vb.)
- ✅ Tüm entity'ler (60+ tablo)

### Infrastructure
- ✅ ApplicationDbContext — IdentityDbContext<AppUser, AppRole, int>
- ✅ ApplicationDbContextFactory — design-time migration
- ✅ Global Query Filter (TenantId, IsDeleted)
- ✅ SaveChanges override (otomatik TenantId/tarih doldurma)
- ✅ **Migration 1:** InitialCreate — tam şema
- ✅ **Migration 2:** AddDealerFlowAndExpandedStatus — bayi akışı + yeni alanlar
- ✅ AesEncryptionService (AES-256-GCM)
- ✅ LocalFileStorageService (düz + şifreli)
- ✅ QrCodeService
- ✅ ZplLabelService (3 şablon: Servis/Aksesuar/Cihaz)
- ✅ SmsService (NetGSM uyumlu)
- ✅ WhatsAppService
- ✅ CurrentTenantService
- ✅ ApplicationDbSeeder (12 AI kuralı)

### Web
- ✅ Program.cs (Cookie auth, SignalR, Hangfire, rate limiter, güvenlik başlıkları)
- ✅ _Layout.cshtml (Metronic/Tailwind, dark mode, toast, SignalR)
- ✅ _SidebarNav.cshtml (tam navigasyon)
- ✅ Login.cshtml

### API
- ✅ Program.cs (JWT Bearer, CORS, OpenAPI .NET 10)
- ✅ GET /health
- ✅ POST /api/v1/auth/login

---

## PHASE 2 — SERVİS ÇEKİRDEĞİ 🔄 DEVAM EDİYOR

### Application CQRS
- ✅ GetServiceRecordsQuery (sayfalama, durum/şube/teknisyen filtresi)
- ✅ GetServiceRecordDetailQuery (tam detay DTO)
- ✅ CreateServiceRecordCommand (kayıt no üretimi, PIN şifreleme, kara liste kontrolü)
- ✅ UpdateServiceStatusCommand (bildirim tetikleyicisi)
- ✅ AddServicePartCommand (fire/israf takibi, IsReplacementOrder)

### Controller
- ✅ ServiceController:
  - Index, Detail, Create GET/POST
  - UpdateStatus, AddPart, PrintLabel
  - PublicQuery (AllowAnonymous), ApprovePrice
  - DealerIncoming, AcceptFromDealer
  - MoveToDealerPool, DealerPool, ShipToDealer

### Views
- ✅ Service/Index.cshtml (sayfalı liste + filtreler)
- ✅ Service/DealerIncoming.cshtml (bayiden teslim bekleyenler)
- ✅ Service/DealerPool.cshtml (bayi sevkiyat havuzu)
- ❌ **Service/Detail.cshtml** — EN KRİTİK EKSİK
  - Durum timeline
  - Durum değiştirme formu (yeni durum + not)
  - Parça ekleme paneli (inline, sayfadan çıkmadan)
  - Ödeme kaydetme paneli
  - Notlar bölümü
  - Medya (fotoğraf) bölümü
  - Bayi iletişim göstergesi
  - Dış servis / kargo takip bilgisi
- ❌ **Service/Create.cshtml** — Cascading AJAX dropdown gerekiyor
  - Tür → Marka → Model → Varyant (AJAX zinciri)
  - `/api/catalog/*` endpoint'leri önce yazılmalı
- ❌ Service/PublicQuery.cshtml (müşteri self-servis, token-based)
- ❌ Service/PublicApproved.cshtml (fiyat onay sonuç sayfası)

---

## PHASE 3 — ALIŞ / SATIŞ ❌ BAŞLAMADI

### Purchase
- ❌ PurchaseController (Normal/Hasarlı/Hurda alış)
- ❌ Views/Purchase/Create.cshtml
  - **Kritik:** Hasarlı alışta (`PurchaseType.Damaged`) → otomatik ServiceRecord oluşturma
  - IMEI kontrolü zorunlu (ImeiChecked = false → kayıt tamamlanamaz)
  - e-devlet PDF upload zorunlu
  - Intake Checklist (admin konfigüreli)
- ❌ Alış sözleşmesi PDF (QuestPDF)
- ❌ Dijital imza entegrasyonu

### Sale
- ❌ SaleController (Cihaz + Aksesuar satışı)
- ❌ Views/Sale/Create.cshtml
- ❌ Satış sözleşmesi PDF

---

## PHASE 4 — BAYİ / TEDARİKÇİ / FİNANS 🔄 KISMEN YAPILDI

### Dealer (Admin Tarafı) ✅
- ✅ DealerController (Index, Create, Detail, Edit, PortalRegister)
- ✅ Views: Index, Create, Detail, PortalRegister

### Dealer Portal ❌
- ❌ DealerPortalController (/bayi/* prefix)
  - GET /bayi/giris → login
  - GET /bayi/pano → dashboard (bekleyen cihazlar, cari bakiye)
  - GET /bayi/cihazlarim → bayi cihaz listesi (masked status)
  - POST /bayi/servis-talebi → yeni servis talebi açma
  - GET /bayi/cari → cari hesap görüntüleme

### Supplier ✅
- ✅ SupplierController (Index, Create, Orders)
- ✅ Views: Supplier/Index, Create, Orders

### Finance ✅
- ✅ FinanceController (Index, Receivables, Payables)
- ✅ Views: Finance/Index, Receivables, Payables

---

## PHASE 5 — STOK / ŞUBE ❌ BAŞLAMADI (Kısmi)

### Stock (Views Hazır, İş Mantığı Eksik)
- ✅ StockController (Parts, Devices, WriteOffs, ScrapSales)
- ✅ Views: Parts.cshtml, Devices.cshtml, WriteOffs.cshtml
- ❌ Aksesuar views (3 kategori: DeviceAccessory, DeviceCompanion, UniversalAccessory)
- ❌ Barkod/QR ile hızlı stok düşme
- ❌ Stok hareketi raporu

### Catalog (Cihaz Kataloğu) ✅
- ✅ CatalogController (DeviceType/Brand/Model/Variant CRUD)
- ✅ Views: Index, Brands, Models, Variants
- ❌ **API Catalog Endpoints** (AJAX için)
  - GET /api/catalog/brands?typeId= 
  - GET /api/catalog/models?brandId=
  - GET /api/catalog/variants?modelId=
  - **PortalRegister.cshtml bu endpoint'leri kullanıyor — YOK = dropdown çalışmıyor**

### Admin
- ✅ AdminController (Users, CustomRoles, Alerts)
- ✅ Views: Admin/Users, CustomRoles, Alerts

---

## PHASE 6 — RAPORLAR / ANALİTİK ❌ BAŞLAMADI

- ❌ ReportController + Views
- ❌ Zaman filtreleri (Bugün/Bu Hafta/Bu Ay/Özel Aralık)
- ❌ Excel export (EPPlus veya ClosedXML)
- ❌ PDF export (QuestPDF)

---

## SONRAKI ADIMLAR (Öncelik Sırasıyla)

### ⚡ Hemen Yapılacak (1. Öncelik)
1. **`GET /api/catalog/brands|models|variants`** — API endpoint'leri
   - Dosya: `src/TeknikServis.Api/Controllers/v1/CatalogController.cs`
   - PortalRegister.cshtml ve Service/Create.cshtml bu endpoint'leri bekliyor

2. **`Service/Create.cshtml`** — Yeni servis formu
   - Cascading dropdown (Tür→Marka→Model→Varyant) AJAX ile
   - Müşteri arama / yeni müşteri popup
   - Sık arıza listesi (CommonFaultType)

3. **`Service/Detail.cshtml`** — Servis detay sayfası
   - Durum timeline (StatusHistories)
   - Inline parça ekleme, ödeme, not
   - Bayi iletişim badge'i

### 🟡 Sonraki Dalga (2. Öncelik)
4. **`HomeController Dashboard`** — KPI kartları
5. **`CustomerController`** + Views (KVKK)
6. **`DealerPortalController`** — Bayi self-service
7. **`PurchaseController`** + Views

### 🟢 Uzun Vadeli (3. Öncelik)
8. Hangfire AI Engine jobs
9. Report modülü (Excel/PDF)
10. Müşteri portalı (/portal/*)

---

## KRITIK TEKNİK NOTLAR

### Razor View Hatırlatıcıları
```
❌ @foreach (var model in Model) → model değişkeni @model direktifiyle çakışır!
✅ @foreach (var m in Model) kullan

❌ <option value="@x.Id" @(cond ? "selected" : "")>  → RZ1031 hatası
✅ if/else blokla ayrı <option selected> ve <option> yaz

❌ Sayfa değişkeni: @page → Razor Pages direktifi gibi parse edilir
✅ @(page) şeklinde parantez ile kullan
```

### Entity Önemli Özellikler
```
TenantId           → Guid (int değil!)
BaseEntity.CreatedAt → DateTime (ChangedAt diye bir şey YOK)
ServiceStatusHistory.ChangedBy → int (nullable değil, 0 gönder)
StockItem.AvailableQuantity → Quantity değil!
StockItem (eşik) → Part.LowStockThreshold (StockItem'da yok)
BlacklistedCustomer → Customer nav var, direkt Phone yok
Customer → FullName property YOK, FirstName + " " + LastName kullan
AlertSeverity → Info(1), Warning(2), Critical(3), Emergency(4) — High/Medium YOK
```

### API Catalog Endpoint Şeması (Henüz Yazılmadı)
```csharp
// PortalRegister.cshtml ve Service/Create.cshtml bu URL'leri bekliyor:
GET /api/catalog/brands?typeId={id}
  → [{ id, name }]

GET /api/catalog/models?brandId={id}
  → [{ id, name, hasVariants }]

GET /api/catalog/variants?modelId={id}
  → [{ id, variantName, modelCode }]
```

### Migration Durumu
- Migration 1: `InitialCreate` ✅
- Migration 2: `AddDealerFlowAndExpandedStatus` ✅
- Sonraki migration gerektiğinde:
  ```bash
  ~/.dotnet/tools/dotnet-ef migrations add <Ad> \
    --project src/TeknikServis.Infrastructure \
    --startup-project src/TeknikServis.Web
  ```

---

## GİT BİLGİSİ

```
Repo:   /home/user/ggg
Branch: claude/erp-device-management-xSpLl
Remote: efnicc-ship-it/ggg

Son commit: feat: kapsamlı servis durum makinesi ve bayi akışı
```
