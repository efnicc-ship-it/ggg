using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Branch;

public class BranchTransfer : BaseEntity, IAuditableEntity
{
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    public string? Notes { get; set; }

    // What is being transferred
    public int? DeviceInventoryId { get; set; }
    public int? PartId { get; set; }
    public int? AccessoryId { get; set; }
    public int? Quantity { get; set; } // for parts/accessories

    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? ReceivedBy { get; set; }
    public string? RejectionReason { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
}
