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
- Her entity `TenantId` (int) taşır
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
- ✅ ServiceStatus enum genişletildi: `Unrepairable=9`, `ReturnedUnrepaired=10`
- ✅ UserRoleAssignment — çoklu rol desteği
- ✅ CustomRole — tenant tanımlı özel roller
- ✅ AppUser — RoleAssignments koleksiyonu eklendi
- ✅ PurchaseRecord — AutoCreatedServiceRecordId, AutoServiceCreated, RepairCostEstimate
- ✅ ServicePart — IsWrittenOff, WriteOffId, IsReplacementOrder (fire takibi)
- ✅ StockWriteOff — fire/israf kaydı (LinkedServiceRecordId ile)
- ✅ ScrapSaleRecord — hurda satışı (HRD-* kayıt numarası)
- ✅ AiRuleDefaults — 12 varsayılan Ensar AI kuralı (static readonly list)

### Application Katmanı
- ✅ IApplicationDbContext — tüm DbSet tanımları
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
  - Tüm yeni DbSet'ler dahil
- ✅ GlobalConfiguration — `decimal(18,4)` precision global uygulama
- ✅ ApplicationDbSeeder — 12 AI kuralı seed (tenant başına)
- ✅ InitialCreate migration (tam şema)
- ✅ AesEncryptionService — AES-256-GCM (nonce+tag+cipher)
- ✅ LocalFileStorageService — düz + şifreli yükleme/indirme
- ✅ QrCodeService — PNG QR üretimi
- ✅ ZplLabelService — TCP ZPL 3 şablon (Servis/Aksesuar/Cihaz)
- ✅ SmsService — HTTP provider-agnostik (NetGSM uyumlu)
- ✅ WhatsAppService — WhatsApp Business API
- ✅ CurrentTenantService — claim okuma (TenantId/BranchId/RegionId/Role)

### Web Katmanı
- ✅ Program.cs — Cookie auth, SignalR, Hangfire, güvenlik başlıkları, rate limiter, session
- ✅ NotificationHub + ChatHub (SignalR)
- ✅ HangfireAuthFilter — sadece SuperAdmin/TenantOwner
- ✅ _Layout.cshtml — tam Metronic/Tailwind layout (sidebar, dark mode, toast, SignalR)
- ✅ _SidebarNav.cshtml — tam navigasyon menüsü
- ✅ Login.cshtml — standalone login (glassmorphism kart)
- ✅ AccountController — Login/Logout, multi-rol claim
- ✅ ServiceController — Index, Detail, Create/POST, UpdateStatus, AddPart, PrintLabel, PublicQuery (AllowAnonymous), ApprovePrice
- ✅ Service/Index.cshtml — sayfalı liste, filtreler, durum hızlı filtreleri

### API Katmanı
- ✅ Program.cs — JWT Bearer, CORS, rate limiter, built-in OpenAPI (.NET 10)
- ✅ HealthController — GET /health
- ✅ AuthController — POST /api/v1/auth/login → JWT (TenantId/BranchId/Role claims)

---

## Yapılacaklar Listesi (Öncelik Sırasıyla)

### Yüksek Öncelik
- [ ] Service/Detail.cshtml — tam servis detay sayfası (durum timeline, parçalar, ödemeler, notlar, medya)
- [ ] Service/Create.cshtml — yeni servis formu (cascading device dropdowns)
- [ ] Service/PublicQuery.cshtml + PublicApproved.cshtml — müşteri self-servis sayfaları
- [ ] PurchaseController + Views — arızalı alışta otomatik servis kaydı oluşturma
- [ ] Customer/Index + Detail + Create (KVKK uyumlu, şifrelenmiş alanlar)
- [ ] HomeController Dashboard — KPI kartları (günlük, haftalık istatistikler)

### Orta Öncelik
- [ ] DeviceCatalog yönetimi — DeviceType → Brand → Model → Variant cascading dropdowns
- [ ] DeviceInventory — ikinci el cihaz stoğu CRUD
- [ ] Sale modülü — SaleController + Views (normal + hurda satışı ayrımı)
- [ ] Stock modülü — parça + 3 aksesuar kategorisi (Companion, Universal, Accessory)
- [ ] Supplier modülü — tedarikçi + parça siparişi (WhatsApp entegrasyonu)
- [ ] Dealer modülü — bayi cari hesap yönetimi
- [ ] Finance modülü — hesap, işlem, alacak/borç

### Düşük Öncelik
- [ ] Hangfire Jobs — Ensar AI kural motoru (12 kural, CronExpression ile), medya temizleme (2 yıl), düşük stok uyarıları
- [ ] Report modülü — servis raporları (iade/iptal ayrı görünüm)
- [ ] Admin paneli — rol/kullanıcı/AI kural/tenant ayarları
- [ ] Randevu modülü (AppointmentRecord)

---

## Yarım Kalan / Devam Edilecek Şeyler

1. **Service Views**: Index.cshtml hazır → Detail, Create, PublicQuery, PublicApproved eksik
2. **PurchaseController**: Arızalı cihaz alışında `PurchaseType == Damaged` olduğunda otomatik ServiceRecord oluşturma mantığı Web katmanında Controller/Handler seviyesinde yazılacak
3. **AddServicePartCommand Fire Flow**: Handler yazıldı ancak `StockWriteOff` oluşturma mantığı eklenecek
4. **Hangfire AI Engine**: `AiRuleDefaults` seed'lendi fakat execution engine (job) henüz yazılmadı

---

## Önemli Notlar ve Edge Case'ler

### Güvenlik
- TC Kimlik No, kimlik kartı fotoğrafı, cihaz PIN → **AES-256-GCM şifreli** depolanır
- KVKK gereği şifreli alanlar: `Customer.IdentityNumberEncrypted`, `AppUser.IdPhotoPathEncrypted`
- Public servis sorgulama sayfası (`/servis/{token}`) login gerektirmez, `ApprovalToken` (GUID) ile güvenli
- Hangfire dashboard → sadece SuperAdmin/TenantOwner rolü erişebilir

### Multi-Tenant Kritik Noktalar
- Tüm entity'lerde `TenantId` alanı zorunlu
- EF Core Global Query Filter sayesinde cross-tenant veri sızıntısı önlenir
- Migration'lar Infrastructure projesinden yönetilir: `dotnet ef migrations add <name> --project src/TeknikServis.Infrastructure --startup-project src/TeknikServis.Web`
- Migration komutu için Web projesinde `Microsoft.EntityFrameworkCore.Design 9.0.4` paketi gerekli

### Domain Kuralları
- Arızalı cihaz alışında (`PurchaseType.Damaged`) → otomatik ServiceRecord oluşturulur, müşteri bilgisi firmanın kendi bilgisi olur
- Onarım tamamlandığında → `DeviceInventory`'ye otomatik işlenir (ikinci el stoğu)
- Fire/israf durumunda (`StockWriteOff`) → aynı servis kaydına yeni parça eklenir, `IsReplacementOrder = true`, maliyet birikir, AI yöneticiye bildirir
- Hurda satış → `ScrapSaleRecord` (HRD-* prefix), normal satıştan ayrı raporlanır
- Servis iade/iptal → `Cancelled` veya `ReturnedUnrepaired` durumları raporlarda ayrı gösterilir

### Bilinen Sorunlar / Dikkat Edilecekler
- `dotnet-ef` CLI aracı global olarak kuruldu: `dotnet tool install --global dotnet-ef` (v10.0.5)
- Domain ve Application projeleri `<FrameworkReference Include="Microsoft.AspNetCore.App" />` kullanır (NuGet paketi değil) — IdentityUser<TKey> tipi için zorunlu
- API projesinde Swashbuckle kaldırıldı, yerine .NET 10 built-in `AddOpenApi()` / `MapOpenApi()` kullanılıyor
- ZPL etiket şablonunda fiyat bilgisi yer almaz (kasıtlı tasarım kararı)

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

---

## Proje Yapısı Özeti

```
ggg/
├── CLAUDE.md                          ← Bu dosya
├── src/
│   ├── TeknikServis.Domain/           ← Entity, Enum, sözleşmeler
│   ├── TeknikServis.Application/      ← CQRS, DTO, Interface
│   ├── TeknikServis.Infrastructure/   ← DbContext, Migration, Servisler
│   ├── TeknikServis.Web/              ← MVC, Views, Cookie auth
│   └── TeknikServis.Api/              ← REST API, JWT auth
└── TeknikServis.sln
```
