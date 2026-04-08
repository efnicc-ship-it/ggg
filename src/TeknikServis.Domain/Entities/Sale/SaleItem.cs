using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Sale;

public class SaleItem : BaseEntity
{
    public int SaleRecordId { get; set; }

    // Ürün tipi — yalnızca biri dolu olur
    public int? DeviceInventoryId { get; set; }   // Cihaz satışı
    public int? DeviceAccessoryId { get; set; }   // Cihaza özgü aksesuar
    public int? DeviceCompanionId { get; set; }   // BT cihazı
    public int? UniversalAccessoryId { get; set; }// Evrensel aksesuar

    public string ItemName { get; set; } = string.Empty;  // Snapshot
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal LineTotal { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
}
