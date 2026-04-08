using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Purchase;

public class PurchaseRecord : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string RecordNumber { get; set; } = string.Empty; // ALI-2024-001234
    public int CustomerId { get; set; }
    public int? BranchId { get; set; }
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public PurchaseType PurchaseType { get; set; } = PurchaseType.Normal;

    // Cihaz detayları
    public string Imei1 { get; set; } = string.Empty;  // ZORUNLU
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? Color { get; set; }
    public int? BatteryHealth { get; set; }

    // IMEI kontrolü (ZORUNLU — olmadan kayıt tamamlanamaz)
    public bool ImeiChecked { get; set; } = false;
    public DateTime? ImeiCheckDate { get; set; }
    public string? ImeiCheckPdfPath { get; set; }       // e-devlet PDF

    // İkinci el için orijinal fatura bilgileri
    public DateTime? OriginalPurchaseDate { get; set; }
    public int? OriginalWarrantyMonths { get; set; }

    // Fiyat
    public decimal PurchasePrice { get; set; }

    // Belgeler
    public string? ContractPdfPath { get; set; }
    public bool LabelPrinted { get; set; } = false;

    // Hurda
    public bool PartsHarvested { get; set; } = false;  // Hurda → parçalar stoğa eklendi mi?

    // Hasarlı → servis bağlantısı
    public int? LinkedServiceRecordId { get; set; }

    // Intake checklist
    public bool IntakeChecklistCompleted { get; set; } = false;
    public string? IntakeChecklistJson { get; set; }
    public string? PhysicalConditionNotes { get; set; }

    // Cihaz envanteri bağlantısı
    public int? DeviceInventoryId { get; set; }

    // Arızalı alışta otomatik açılan servis kaydı
    // PurchaseType == Damaged → sistem otomatik ServiceRecord açar, müşteri bilgileri şirketin kendi bilgileriyle doldurulur
    public int? AutoCreatedServiceRecordId { get; set; }
    public bool AutoServiceCreated { get; set; } = false;

    // İşlem maliyeti ve açıklama (özellikle arızalı alışlar için)
    public decimal? RepairCostEstimate { get; set; }
    public string? RepairDescription { get; set; }   // Yapılan/yapılacak işlem açıklaması

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public Customer.Customer Customer { get; set; } = null!;
    public Device.DeviceModel DeviceModel { get; set; } = null!;
    public Device.DeviceModelVariant? DeviceModelVariant { get; set; }
    public ICollection<PurchaseDocument> Documents { get; set; } = new List<PurchaseDocument>();
}
