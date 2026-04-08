using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

public class Part : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Sku { get; set; }
    public string? Description { get; set; }
    public int LowStockThreshold { get; set; } = 3;
    public decimal LastPurchasePrice { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public ICollection<PartCompatibility> Compatibilities { get; set; } = new List<PartCompatibility>();
    public StockItem? StockItem { get; set; }
}
