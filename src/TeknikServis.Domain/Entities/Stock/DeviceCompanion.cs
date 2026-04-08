using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

/// <summary>
/// Cihazla birlikte kullanılan elektronik — Bluetooth kulaklık, BT hoparlör, vb.
/// Belirli bir marka/modelle birlikte kullanılır ama universal değil
/// </summary>
public class DeviceCompanion : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }                   // BT kulaklık markası
    public string? Model { get; set; }                   // BT kulaklık modeli
    public string? ConnectivityType { get; set; }        // "Bluetooth" | "USB-C" | "Lightning"
    public string? ImagePath { get; set; }
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public int LowStockThreshold { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    public bool IsListedOnStorefront { get; set; } = false;
    public string? StorefrontSlug { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public StockItem? StockItem { get; set; }
}
