using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Finance;

public class DealerReceivable : BaseEntity, IAuditableEntity
{
    public int DealerId { get; set; }
    public int? BranchId { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // "ServiceRecord" | "SaleRecord"
    public int ReferenceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsDeferred { get; set; } = false;
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Dealer.Dealer Dealer { get; set; } = null!;
}
