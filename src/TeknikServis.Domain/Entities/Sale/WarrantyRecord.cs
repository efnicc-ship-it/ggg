using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Sale;

public class WarrantyRecord : BaseEntity, IAuditableEntity
{
    public int SaleRecordId { get; set; }
    public int? DeviceInventoryId { get; set; }
    public int? CustomerId { get; set; }
    public int WarrantyDays { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public WarrantyStatus Status { get; set; } = WarrantyStatus.Active;
    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
}
