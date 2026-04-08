using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

/// <summary>
/// Evrensel aksesuar — Şarj aleti, kablo, adaptör, vb.
/// Tüm cihazlarla veya bağımsız kullanılır
/// </summary>
public class UniversalAccessory : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public string? ConnectorType { get; set; }   // "USB-C", "Lightning", "Micro-USB", "Universal"
    public string? Brand { get; set; }
    public string? ImagePath { get; set; }
    public decimal SalePrice { get; set; }
    public decimal PurchasePrice { get; set; }
    public int LowStockThreshold { get; set; } = 5;
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
