using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Supplier;

public class PartOrderItem : BaseEntity, IAuditableEntity
{
    public int PartOrderId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public PartOrderStatus Status { get; set; } = PartOrderStatus.Ordered;
    public DateTime? ReceivedAt { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public PartOrder PartOrder { get; set; } = null!;
    public Stock.Part Part { get; set; } = null!;
}
