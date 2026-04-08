using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Device;

public class DeviceInventory : BaseEntity, IAuditableEntity, ISoftDelete
{
    public int CatalogId { get; set; }
    public int? BranchId { get; set; }
    public DeviceCondition Condition { get; set; } = DeviceCondition.SecondHand;
    public InventorySource Source { get; set; } = InventorySource.CustomerPurchase;
    public InventoryStatus Status { get; set; } = InventoryStatus.Available;

    // Cihaz tanımlayıcılar
    public string? Imei1 { get; set; }
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? Color { get; set; }

    // Fiziksel durum
    public string? PhysicalConditionNotes { get; set; }
    public int? BatteryHealth { get; set; }           // % değer (0-100)

    // IMEI kontrolü
    public bool ImeiChecked { get; set; } = false;
    public DateTime? ImeiCheckDate { get; set; }
    public string? ImeiCheckPdfPath { get; set; }     // e-devlet PDF

    // Intake checklist
    public bool IntakeChecklistCompleted { get; set; } = false;
    public string? IntakeChecklistJson { get; set; }  // JSON: her maddenin tik durumu

    // Fiyat bilgileri
    public decimal PurchasePrice { get; set; }
    public decimal TargetSalePrice { get; set; }

    // Kaynak bilgisi (kim sattı?)
    public int? PurchasedFromCustomerId { get; set; }
    public int? PurchasedFromSupplierId { get; set; }
    public DateTime PurchasedAt { get; set; }

    // İkinci el için orijinal fatura bilgileri
    public DateTime? OriginalPurchaseDate { get; set; }
    public int? OriginalWarrantyMonths { get; set; }

    // Bağlantılı kayıtlar
    public int? LinkedPurchaseRecordId { get; set; }  // Alış kaydıyla bağlantı
    public int? LinkedSaleRecordId { get; set; }      // Satış kaydıyla bağlantı

    // Label
    public bool LabelPrinted { get; set; } = false;

    // Vitrin
    public bool IsListedOnStorefront { get; set; } = false;
    public string? StorefrontSlug { get; set; }       // /u/{slug}

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public DeviceCatalog Catalog { get; set; } = null!;
}
