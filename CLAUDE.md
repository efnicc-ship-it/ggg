# TeknikServis ERP — Proje Dokümantasyonu

## Projenin Amacı ve Hedef Kullanıcılar

Türkiye'deki telefon/tablet/notebook teknik servis + ikinci el alım/satım + aksesuar işletmeleri için tam kapsamlı, çok kiracılı (multi-tenant) SaaS ERP sistemi.

**Hedef kullanıcılar:**
- Tek kişilik ufak teknik servis atölyeleri (tüm rolleri aynı anda üstlenen bireyler)
- Çok şubeli zincir teknik servis firmaları
- İkinci el cihaz alım-satım yapan firmalar
- Bayi ağları aracılığıyla çalışan servis merkezleri

---

## Mimari Yapı ve Teknoloji Kararları

### Katmanlı Mimari (Clean Architecture)
```
TeknikServis.Domain        → Entity, Enum, ortak sözleşmeler
TeknikServis.Application   → CQRS (MediatR), Validator, DTO, Interface tanımları
TeknikServis.Infrastructure → EF Core DbContext, Identity, Hangfire, dış servisler
TeknikServis.Web           → ASP.NET Core MVC (Razor Views, Cookie auth)
TeknikServis.Api           → REST API (JWT Bearer auth)
```

### Teknoloji Seçimleri ve Gerekçeleri

| Teknoloji | Karar | Neden |
|-----------|-------|-------|
| **ASP.NET Core 10 MVC** | Web uygulaması | Olgun, yüksek performanslı, Razor ile SSR |
| **ASP.NET Core Web API** | REST API katmanı | Mobil/3. taraf entegrasyon için ayrı JWT context |
| **MSSQL + EF Core 9 Code-First** | Veritabanı | Güçlü ilişkisel model, migration yönetimi |
| **MediatR + CQRS** | İş mantığı | Command/Query ayrımı, Pipeline Behaviors |
| **FluentValidation** | Doğrulama | Temiz, okunabilir kural tanımları |
| **AutoMapper** | DTO dönüşümü | IMapFrom<T> convention taraması |
| **ASP.NET Core Identity** | Kimlik yönetimi | IdentityUser<int> genişletildi (AppUser) |
| **Hangfire + MSSQL** | Arka plan işleri | Ensar AI kural motoru, medya temizleme |
| **SignalR** | Gerçek zamanlı | Bildirimler (NotificationHub), dahili chat (ChatHub) |
| **AES-256-GCM** | KVKK şifreleme | TC Kimlik No, kimlik fotoğrafı, cihaz PIN |
| **ZPL / TCP socket** | Termal etiket | Servis/Aksesuar/Cihaz etiket şablonları |
| **Tailwind CSS CDN + Alpine.js** | Frontend | Metronic Tailwind Demo6 stili, dark mode |
| **JWT Bearer** | API auth | Multi-device/account, claim-based tenant |
| **Cookie auth** | Web auth | Session tabanlı, CSRF korumalı |
| **QRCoder** | QR kod | Servis ve ürün QR etiketleri |
| **Serilog** | Loglama | Yapılandırılabilir sink'ler (File, Console) |

### Multi-Tenant Yaklaşımı
- Her entity `TenantId` (Guid) taşır
- EF Core **Global Query Filter** → tüm sorgular otomatik tenant-izole
- `ICurrentUserService` claim'lerden TenantId/BranchId/Role okur
- `SaveChangesAsync` override → TenantId otomatik doldurulur

### Çoklu Rol Sistemi
- `UserRoleAssignment` tablosu: bir kullanıcı N rol taşıyabilir (ExpiresAt desteği)
- `CustomRole` entity: tenant admin yeni rol tanımlayabilir, PermissionsJson ile izin listesi
- Tek kişilik işletme: aynı anda Patron + Teknisyen + Kasiyer rolü

---

## Tamamlanan Özellikler ✅

### Domain Katmanı
- ✅ Tüm entity'ler (Service, Purchase, Sale, Stock, Customer, Dealer, Supplier, Finance, Device, EnsarAI, Identity, Tenant, Audit, Campaign, Appointment, InternalChat)
- ✅ **ServiceStatus enum TAM GENIŞLETILDI** — 17 durum (bkz. aşağı)
- ✅ UserRoleAssignment — çoklu rol desteği
- ✅ CustomRole — tenant tanımlı özel roller
- ✅ AppUser — RoleAssignments koleksiyonu eklendi
- ✅ PurchaseRecord — AutoCreatedServiceRecordId, AutoServiceCreated, RepairCostEstimate
- ✅ ServicePart — IsWrittenOff, WriteOffId, IsReplacementOrder (fire takibi)
- ✅ StockWriteOff — fire/israf kaydı (LinkedServiceRecordId ile)
- ✅ ScrapSaleRecord — hurda satışı (HRD-* kayıt numarası)
- ✅ AiRuleDefaults — 12 varsayılan Ensar AI kuralı (static readonly list)
- ✅ **Dealer entity** — AutoApprovalLimit, DefaultDealerIsContact, ShowMaskedStatusOnly
- ✅ **DealerBatchShipment entity** — toplu bayi sevkiyat yönetimi (BSH-YYYY-XXXXXX)
- ✅ **ServiceRecord** — DealerIsContactPerson, InboundCargoTrackingNumber, DealerBatchShipmentId, EnteredDealerPoolAt, DealerCreditHoldAt, ExternalServiceProvider alanları

### ServiceStatus Enum — Tam Liste (17 durum)
```
// Kabul ve Giriş
Received = 1            Cihaz Teslim Alındı (standart kabul)
CargoWaiting = 2        Kargo Bekleniyor (uzaktan müşteri/bayi gönderimi yolda)

// Bayi Akışı
DealerRegistered = 20   Bayi Tarafından Kayıt Edildi (fiziksel teslim henüz yok)
DealerInTransit = 21    Bayiden Transfer Aşamasında (kargo yolda)
BatchAcceptancePending = 22  Toplu Kabul / Ayrıştırma Bekliyor

// Teşhis ve Onay
DiagnosisWaiting = 3    Arıza Tespiti Bekleniyor (teknisyen sırasında)
Diagnosing = 4          Arıza Tespiti Aşamasında
AwaitingApproval = 5    Müşteri Onayı Bekleniyor
AwaitingDealerApproval = 23  Bayi Onayı Bekleniyor

// Operasyon ve Onarım
ApprovedWaitingRepair = 6   Onaylandı / İşlem Sırasında Bekliyor
InRepair = 7            Onarım Aşamasında
AwaitingParts = 8       Yedek Parça Bekleniyor
ExternalService = 9     Dış Servise (Taşerona) Yönlendirildi

// Kalite Kontrol
QualityControl = 10     Kalite Kontrol / Test Aşamasında
Ready = 11              Onarım Tamamlandı / Teslime Hazır

// Bayi Çıkış
DealerPool = 24         Bayi Sevkiyat Havuzunda Bekliyor
DealerCreditHold = 25   Cari Limit Engelinde / Ödeme Bekliyor
ShippedToDealer = 26    Bayiye Sevk Edildi

// Kapanış
Delivered = 12          Müşteriye Elden Teslim Edildi
ShippedBack = 13        Müşteriye Kargoya Verildi

// İptal ve Özel
Cancelled = 14          İptal / Onay Verilmedi
Unrepairable = 15       Onarım Mümkün Değil
ReturnedUnrepaired = 16 Onarılmadan İade Edildi
ScrapPending = 17       Hurdaya Ayrılıyor
```

### Application Katmanı
- ✅ IApplicationDbContext — tüm DbSet tanımları (DealerBatchShipments dahil)
- ✅ ICurrentUserService — multi-branch, multi-role claim okuma
- ✅ ValidationBehavior + LoggingBehavior (MediatR pipeline)
- ✅ AutoMapper MappingProfile (IMapFrom<T> convention)
- ✅ Service CQRS:
  - GetServiceRecordsQuery (sayfalama, filtreler)
  - GetServiceRecordDetailQuery (tam detay DTO)
  - CreateServiceRecordCommand (kayıt no, PIN şifreleme, kara liste kontrolü)
  - UpdateServiceStatusCommand (müşteri bildirim tetikleyicisi)
  - AddServicePartCommand (fire: IsReplacementOrder → yönetici bildirimi)

### Infrastructure Katmanı
- ✅ ApplicationDbContext (IdentityDbContext<AppUser,AppRole,int>)
  - SaveChanges override (TenantId, CreatedAt, UpdatedAt, soft delete)
  - Tüm yeni DbSet'ler dahil (DealerBatchShipments)
- ✅ **ApplicationDbContextFactory** — design-time migration factory (NullCurrentUserService)
- ✅ GlobalConfiguration — `decimal(18,4)` precision global uygulama
- ✅ ApplicationDbSeeder — 12 AI kuralı seed (tenant başına)
- ✅ **InitialCreate migration** — tam şema
- ✅ **AddDealerFlowAndExpandedStatus migration** — bayi akışı + genişletilmiş durum
- ✅ AesEncryptionService — AES-256-GCM (nonce+tag+cipher)
- ✅ LocalFileStorageService — düz + şifreli yükleme/indirme
- ✅ QrCodeService — PNG QR üretimi
- ✅ ZplLabelService — TCP ZPL 3 şablon (Servis/Aksesuar/Cihaz)
- ✅ SmsService — HTTP provider-agnostik (NetGSM uyumlu)
- ✅ WhatsAppService — WhatsApp Business API
- ✅ CurrentTenantService — claim okuma (TenantId/BranchId/RegionId/Role)

### Web Katmanı — Controllers
- ✅ Program.cs — Cookie auth, SignalR, Hangfire, güvenlik başlıkları, rate limiter, session
- ✅ NotificationHub + ChatHub (SignalR)
- ✅ HangfireAuthFilter — sadece SuperAdmin/TenantOwner
- ✅ AccountController — Login/Logout, multi-rol claim
- ✅ **ServiceController** — Index, Detail, Create/POST, UpdateStatus, AddPart, PrintLabel, PublicQuery, ApprovePrice + **DealerIncoming, AcceptFromDealer, MoveToDealerPool, DealerPool, ShipToDealer**
- ✅ **DealerController** — Index, Create, Detail, Edit, PortalRegister
- ✅ SupplierController — Index, Create, Orders
- ✅ FinanceController — Index, Receivables, Payables
- ✅ **CatalogController** — DeviceType/Brand/Model/Variant tam CRUD
- ✅ AdminController — Users, CustomRoles, Alerts
- ✅ StockController — Parts, Devices, WriteOffs, ScrapSales

### Web Katmanı — Views
- ✅ _Layout.cshtml — tam Metronic/Tailwind layout (sidebar, dark mode, toast, SignalR)
- ✅ _SidebarNav.cshtml — tam navigasyon menüsü
- ✅ Login.cshtml — standalone login
- ✅ Service/Index.cshtml — sayfalı liste, filtreler, durum hızlı filtreleri
- ✅ **Service/DealerIncoming.cshtml** — bayiden gelen / teslim bekleyen cihazlar
- ✅ **Service/DealerPool.cshtml** — bayi sevkiyat havuzu, toplu sevkiyat formu
- ✅ **Catalog/Index.cshtml** — DeviceType CRUD
- ✅ **Catalog/Brands.cshtml** — DeviceBrand CRUD
- ✅ **Catalog/Models.cshtml** — DeviceModel CRUD
- ✅ **Catalog/Variants.cshtml** — DeviceModelVariant CRUD
- ✅ **Dealer/Index.cshtml** — bayi listesi
- ✅ **Dealer/Create.cshtml** — bayi oluştur (oto onay limiti dahil)
- ✅ **Dealer/Detail.cshtml** — bayi detay, kredi bar, ayarlar
- ✅ **Dealer/PortalRegister.cshtml** — bayi adına servis kaydı açma
- ✅ Supplier/Index.cshtml, Create.cshtml, Orders.cshtml
- ✅ Finance/Index.cshtml, Receivables.cshtml, Payables.cshtml
- ✅ Stock/Parts.cshtml, Devices.cshtml, WriteOffs.cshtml
- ✅ Admin/Users.cshtml, CustomRoles.cshtml, Alerts.cshtml

### API Katmanı
- ✅ Program.cs — JWT Bearer, CORS, rate limiter, built-in OpenAPI (.NET 10)
- ✅ HealthController — GET /health
- ✅ AuthController — POST /api/v1/auth/login → JWT (TenantId/BranchId/Role claims)

---

## Yapılacaklar Listesi (Öncelik Sırasıyla)

### 🔴 Yüksek Öncelik (Devam Edilecek)
- [ ] **Service/Detail.cshtml** — tam detay sayfası (durum timeline, parçalar, ödemeler, notlar, medya, bayi iletişim göstergesi)
- [ ] **Service/Create.cshtml** — yeni servis formu (cascading AJAX dropdowns: Tür→Marka→Model→Varyant)
- [ ] **Service/PublicQuery.cshtml** + PublicApproved.cshtml — müşteri self-servis sayfaları (token-based)
- [ ] **HomeController** Dashboard — KPI kartları (günlük, haftalık istatistikler, bayi cihazları özeti)
- [ ] **Customer/Index.cshtml** + Detail + Create — KVKK uyumlu, TC kimlik şifreli

### 🟡 Orta Öncelik
- [ ] **PurchaseController + Views** — arızalı alışta otomatik ServiceRecord oluşturma
- [ ] **DeviceInventory** CRUD — cihaz envanteri (sıfır + ikinci el), IMEI zorunluluğu
- [ ] **Sale modülü** — SaleController + Views (normal + hurda satışı ayrımı)
- [ ] **Stock modülü** — parça + 3 aksesuar kategorisi views (Companion, Universal, Accessory)
- [ ] **API: /api/catalog/*** — brands, models, variants AJAX endpoints (Service/Create ve PortalRegister için gerekli)
- [ ] **Bayi Portali** — /bayi/* prefix'i altında ayrı DealerPortalController (login, dashboard, cihazlarım, servis talebi, cari hesap)

### 🟢 Düşük Öncelik
- [ ] Hangfire Jobs — Ensar AI kural motoru (12 kural), medya temizleme (2 yıl), düşük stok uyarıları
- [ ] Report modülü — kapsamlı raporlar + Excel/PDF export
- [ ] Admin paneli — tenant/modül yönetimi
- [ ] Randevu modülü (AppointmentRecord)
- [ ] Müşteri portalı /portal/* — müşteri self-service (tüm geçmiş)

---

## Bayi Akışı — İş Kuralları (Kritik)

### İletişim Kişisi Seçimi
- `Dealer.DefaultDealerIsContact = true` → kayıt açılırken varsayılan bayi iletişim kişisi
- `ServiceRecord.DealerIsContactPerson = true` → bu kayıt için fiyat onayı/bildirimler bayiye gider
- `ServiceRecord.DealerIsContactPerson = false` → servis merkezi müşteriyle doğrudan iletişim kurar
- Bayi adına kayıt açılırken (`PortalRegister`) kayıt bazında seçim yapılabilir

### Otomatik Onay Limiti
- `Dealer.AutoApprovalLimit = null` → tüm işlemler `AwaitingDealerApproval` bekler
- `Dealer.AutoApprovalLimit = 500` → 500 TL ve altı işlemler direkt `ApprovedWaitingRepair`'e geçer
- 500 TL üstü işlemler → `AwaitingDealerApproval` bekler

### Maskelenmiş Durum Gösterimi
- `Dealer.ShowMaskedStatusOnly = true` → Bayi panelinde operasyonel detaylar gizlenir
- Bayi sadece genel durum görür: "Merkezde İşlem Görüyor", "Teslime Hazır", "Sevk Edildi"
- Merkez detaylı görür: "Dış Serviste", "Kalite Kontrolde", vb.

### Bayi Akış Durumları
```
[Bayi kayıt açar] → DealerRegistered
[Kargo verir]     → DealerInTransit  (kargo takip no varsa otomatik)
[Kargo gelir]     → BatchAcceptancePending
[Teslim alınır]   → Received  (AcceptFromDealer action)
     ↓ (normal servis akışı)
[Hazır olunca]    → DealerPool  (MoveToDealerPool action)
[Limit aşımı]     → DealerCreditHold  (otomatik kontrol)
[Sevkiyat]        → ShippedToDealer  (ShipToDealer, DealerBatchShipment oluşur)
```

### Cari Limit Kontrolü
- `MoveToDealerPool` çağrıldığında: `CurrentBalance + FinalPrice > CreditLimit` → `DealerCreditHold`
- Ödeme alındıktan sonra `CurrentBalance` güncellenir, ardından `MoveToDealerPool` tekrar çağrılır

---

## Yarım Kalan / Devam Edilecek Şeyler

1. **Service/Detail.cshtml** — En kritik eksik view. Durum değiştirme formu, parça paneli, ödeme paneli, timeline
2. **Service/Create.cshtml** — AJAX dropdown zinciri (Tür→Marka→Model→Varyant). `/api/catalog/*` endpoint'leri önce yazılmalı
3. **API Catalog Endpoints** — `GET /api/catalog/brands?typeId=`, `GET /api/catalog/models?brandId=`, `GET /api/catalog/variants?modelId=` — PortalRegister ve Create formlarında kullanılıyor (şu an JavaScript fetch çağrıları var ama endpoint'ler yok)
4. **PurchaseController**: Arızalı cihaz alışında `PurchaseType == Damaged` → otomatik ServiceRecord oluşturma mantığı
5. **AddServicePartCommand Fire Flow**: Handler yazıldı ancak `StockWriteOff` oluşturma mantığı eklenecek
6. **Hangfire AI Engine**: `AiRuleDefaults` seed'lendi fakat execution engine (job) henüz yazılmadı
7. **DealerPortalController**: Bayi kendi panelinden cihaz görmeli, servis talebi açabilmeli, cari hesabını görmeli

---

## Önemli Notlar ve Edge Case'ler

### Güvenlik
- TC Kimlik No, kimlik kartı fotoğrafı, cihaz PIN → **AES-256-GCM şifreli** depolanır
- KVKK gereği şifreli alanlar: `Customer.IdentityNumberEncrypted`, `AppUser.IdPhotoPathEncrypted`
- Public servis sorgulama sayfası (`/servis/{token}`) login gerektirmez, `ApprovalToken` (GUID) ile güvenli
- Hangfire dashboard → sadece SuperAdmin/TenantOwner rolü erişebilir

### Migration Komutları
```bash
# Migration oluştur
~/.dotnet/tools/dotnet-ef migrations add <MigrationAdi> \
  --project src/TeknikServis.Infrastructure \
  --startup-project src/TeknikServis.Web

# Veritabanı güncelle
~/.dotnet/tools/dotnet-ef database update \
  --project src/TeknikServis.Infrastructure \
  --startup-project src/TeknikServis.Web
```

### Build Komutu
```bash
dotnet build src/TeknikServis.Web/TeknikServis.Web.csproj
```

### Bilinen Sorunlar / Dikkat Edilecekler
- `dotnet-ef` CLI aracı: `~/.dotnet/tools/dotnet-ef` (global, PATH'te olmayabilir)
- Domain ve Application projeleri `<FrameworkReference Include="Microsoft.AspNetCore.App" />` kullanır (NuGet paketi değil)
- API projesinde Swashbuckle kaldırıldı, yerine .NET 10 built-in `AddOpenApi()` / `MapOpenApi()` kullanılıyor
- ZPL etiket şablonunda fiyat bilgisi yer almaz (kasıtlı tasarım kararı)
- Razor view'larda loop değişkeni `model` adı verilmemelidir — `@model` direktifi ile çakışır
- `<option>` tag helper içinde `@(cond ? "selected" : "")` → RZ1031 hatası; if/else blok kullan

### Ortam Değişkenleri (Production'da zorunlu)
```
Encryption__AesKey          = <32-byte base64 key>
ConnectionStrings__DefaultConnection = <production MSSQL>
Sms__Username / Sms__Password
WhatsApp__Token
```

### Veritabanı Nüansları
- Tüm `decimal` kolonlar `decimal(18,4)` — EF Core precision uyarılarını bastırır
- Soft delete: `DeletedAt` + `IsDeleted` alanları, Global Query Filter ile otomatik filtre
- `TenantId` tipi: `Guid` (int değil — BaseEntity'de Guid olarak tanımlı)

---

## Proje Yapısı Özeti

```
ggg/
├── CLAUDE.md                          ← Bu dosya (her session sonunda güncellenir)
├── docs/
│   └── PROGRESS.md                    ← Detaylı ilerleme takibi
├── src/
│   ├── TeknikServis.Domain/           ← Entity, Enum, sözleşmeler
│   ├── TeknikServis.Application/      ← CQRS, DTO, Interface
│   ├── TeknikServis.Infrastructure/   ← DbContext, Migration, Servisler
│   ├── TeknikServis.Web/              ← MVC, Views, Cookie auth
│   └── TeknikServis.Api/              ← REST API, JWT auth
└── TeknikServis.sln
```

---

## Devam Talimatı (Her Yeni Session İçin)

Yeni bir Claude session'ında şunu söyle:

> "TeknikServis ERP projesine devam ediyoruz.
> Repo: `/home/user/ggg`
> Branch: `claude/erp-device-management-xSpLl`
> CLAUDE.md ve docs/PROGRESS.md dosyalarını oku, kaldığımız yerden devam et."

Claude şunları yapacak:
1. CLAUDE.md okuyacak (bu dosya)
2. docs/PROGRESS.md okuyacak (detaylı ilerleme)
3. Bir sonraki açık tasktan devam edecek
