using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Stock;

public class StockMovement : BaseEntity, IAuditableEntity
{
    public int StockItemId { get; set; }
    public int? BranchId { get; set; }
    public string MovementType { get; set; } = string.Empty;  // "In" | "Out" | "Return" | "Transfer" | "Adjustment"
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; } = 0;
    public string? ReferenceType { get; set; }  // "ServiceRecord" | "PartOrder" | "SaleRecord" | "BranchTransfer"
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public StockItem StockItem { get; set; } = null!;
}
