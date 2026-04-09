using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Dealer;

/// <summary>
/// Bayiye yapılan toplu sevkiyat kaydı.
/// Birden fazla cihaz tek irsaliye/fatura altında gönderilir.
/// </summary>
public class DealerBatchShipment : BaseEntity, IAuditableEntity
{
    public int DealerId { get; set; }

    // Sevkiyat bilgileri
    public string ShipmentNumber { get; set; } = string.Empty; // BSH-2024-000001
    public DateTime ShippedAt { get; set; }
    public string? CargoCompany { get; set; }
    public string? CargoTrackingNumber { get; set; }

    // Finansal
    public decimal TotalServiceAmount { get; set; } = 0;  // Toplam servis tutarı
    public bool InvoiceCreated { get; set; } = false;
    public string? InvoiceNumber { get; set; }

    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // Navigation
    public Dealer Dealer { get; set; } = null!;
    public ICollection<Service.ServiceRecord> ServiceRecords { get; set; } = new List<Service.ServiceRecord>();
}
