using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Finance;

public class CustomerReceivable : BaseEntity, IAuditableEntity
{
    public int CustomerId { get; set; }
    public int? BranchId { get; set; }
    public string ReferenceType { get; set; } = string.Empty; // "ServiceRecord" | "SaleRecord"
    public int ReferenceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; } = false;
    public DateTime? LastReminderSentAt { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Customer.Customer Customer { get; set; } = null!;
}
