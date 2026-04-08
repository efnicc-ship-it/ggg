using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Branch;

public class ServiceBranchTransfer : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public int FromBranchId { get; set; }
    public int ToBranchId { get; set; }
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public int? ReceivedBy { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Branch FromBranch { get; set; } = null!;
    public Branch ToBranch { get; set; } = null!;
}
