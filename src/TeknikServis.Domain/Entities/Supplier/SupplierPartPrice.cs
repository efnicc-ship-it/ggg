using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Supplier;

public class SupplierPartPrice : BaseEntity
{
    public int SupplierId { get; set; }
    public int PartId { get; set; }
    public decimal UnitPrice { get; set; }
    public int? LeadTimeDays { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; }  // Fiyat tarihi

    public Supplier Supplier { get; set; } = null!;
    public Stock.Part Part { get; set; } = null!;
}
