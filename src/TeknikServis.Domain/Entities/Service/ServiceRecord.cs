using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Service;

public class ServiceRecord : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string RecordNumber { get; set; } = string.Empty; // SRV-2024-001234
    public int CustomerId { get; set; }
    public int? DealerId { get; set; }
    public int? BranchId { get; set; }

    // Cihaz bilgileri
    public int DeviceModelId { get; set; }
    public int? DeviceModelVariantId { get; set; }
    public string? Imei1 { get; set; }
    public string? Imei2 { get; set; }
    public string? SerialNumber { get; set; }
    public string? DeviceColor { get; set; }
    public string? DevicePasswordEncrypted { get; set; }  // AES-256 şifreli PIN/şifre

    // Arıza bilgileri
    public string FaultDescription { get; set; } = string.Empty;
    public string? InternalNotes { get; set; }            // Müşteriye gösterilmez

    // Durum yönetimi
    public ServiceStatus Status { get; set; } = ServiceStatus.Received;
    public int? AssignedTechnicianId { get; set; }

    // Garanti
    public bool IsUnderWarranty { get; set; } = false;
    public int? RelatedWarrantyRecordId { get; set; }

    // Fiyat onayı
    public string? ApprovalToken { get; set; }            // GUID — public sorgu için
    public bool? PriceApproved { get; set; }
    public decimal? QuotedPrice { get; set; }
    public decimal? FinalPrice { get; set; }
    public DateTime? ApprovalSentAt { get; set; }
    public DateTime? ApprovalRespondedAt { get; set; }

    // Ödeme
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public decimal TotalPaid { get; set; } = 0;

    // Maliyet hesabı
    public decimal PartsCost { get; set; } = 0;          // Kullanılan parça maliyeti
    public decimal LaborCost { get; set; } = 0;          // İşçilik maliyeti
    public decimal NetProfit { get; set; } = 0;          // FinalPrice - (PartsCost + LaborCost)

    // Teslim ve etiket
    public bool LabelPrinted { get; set; } = false;
    public DateTime? DeliveredAt { get; set; }

    // YouTube yayın
    public bool LiveStreamPlanned { get; set; } = false;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // Navigation properties
    public Customer.Customer Customer { get; set; } = null!;
    public Device.DeviceModel DeviceModel { get; set; } = null!;
    public Device.DeviceModelVariant? DeviceModelVariant { get; set; }
    public ICollection<ServiceStatusHistory> StatusHistories { get; set; } = new List<ServiceStatusHistory>();
    public ICollection<ServiceMedia> Media { get; set; } = new List<ServiceMedia>();
    public ICollection<ServiceNote> Notes { get; set; } = new List<ServiceNote>();
    public ICollection<ServicePart> Parts { get; set; } = new List<ServicePart>();
    public ICollection<ServicePayment> Payments { get; set; } = new List<ServicePayment>();
    public ICollection<ServiceTimeLog> TimeLogs { get; set; } = new List<ServiceTimeLog>();
    public ICollection<PriceApprovalLog> PriceApprovalLogs { get; set; } = new List<PriceApprovalLog>();
    public LiveStreamSchedule? LiveStreamSchedule { get; set; }
}
