using TeknikServis.Domain.Common;
using TeknikServis.Domain.Entities.Device;

namespace TeknikServis.Domain.Entities.Stock;

/// <summary>
/// Fire kaydı — parça, ikinci el cihaz veya aksesuar stoktan düşülürken fire nedeni belgelenir.
/// Örn: Ekran takılırken kırıldı → fire, yeni sipariş verildi → maliyet artar.
/// </summary>
public class StockWriteOff : BaseEntity, IAuditableEntity
{
    public int? BranchId { get; set; }

    // Ne fire edildi (biri dolu olur)
    public int? PartId { get; set; }
    public int? DeviceAccessoryId { get; set; }
    public int? DeviceCompanionId { get; set; }
    public int? UniversalAccessoryId { get; set; }
    public int? DeviceInventoryId { get; set; }  // İkinci el cihaz fire/hurda

    public int Quantity { get; set; } = 1;
    public decimal UnitCost { get; set; }            // Fire maliyeti
    public decimal TotalCost { get; set; }

    public string Reason { get; set; } = string.Empty;
    // "InstallationDamage" | "TransportDamage" | "DefectiveProduct" | "Expired" | "Other"
    public string? Notes { get; set; }

    // Bağlı servis kaydı (takma sırasında kırılan parça)
    public int? LinkedServiceRecordId { get; set; }

    // Bağlı parça siparişi (fire nedeniyle yeniden sipariş verilen)
    public int? LinkedPartOrderItemId { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Part? Part { get; set; }
    public DeviceAccessory? DeviceAccessory { get; set; }
    public DeviceCompanion? DeviceCompanion { get; set; }
    public UniversalAccessory? UniversalAccessory { get; set; }
    public TeknikServis.Domain.Entities.Device.DeviceInventory? DeviceInventory { get; set; }
}
