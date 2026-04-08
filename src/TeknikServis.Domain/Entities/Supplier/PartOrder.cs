using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Supplier;

public class PartOrder : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string OrderNumber { get; set; } = string.Empty;  // SPR-2024-001234
    public int SupplierId { get; set; }
    public int? ServiceRecordId { get; set; }   // Hangi servis kaydından sipariş verildi
    public int? BranchId { get; set; }
    public PartOrderStatus Status { get; set; } = PartOrderStatus.Ordered;
    public decimal TotalAmount { get; set; }
    public bool WhatsAppSent { get; set; } = false;
    public DateTime? WhatsAppSentAt { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    public Supplier Supplier { get; set; } = null!;
    public ICollection<PartOrderItem> Items { get; set; } = new List<PartOrderItem>();
}
