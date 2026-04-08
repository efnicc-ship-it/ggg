using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

public class StockItem : BaseEntity, IAuditableEntity
{
    public int? BranchId { get; set; }

    // Sadece biri dolu olur (hangi ürün türü)
    public int? PartId { get; set; }
    public int? DeviceAccessoryId { get; set; }
    public int? DeviceCompanionId { get; set; }
    public int? UniversalAccessoryId { get; set; }

    public int AvailableQuantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;   // Servis için ayrılan
    public int TotalQuantity => AvailableQuantity + ReservedQuantity;
    public decimal AverageCost { get; set; } = 0;    // Ağırlıklı ortalama maliyet

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Part? Part { get; set; }
    public DeviceAccessory? DeviceAccessory { get; set; }
    public DeviceCompanion? DeviceCompanion { get; set; }
    public UniversalAccessory? UniversalAccessory { get; set; }
}
