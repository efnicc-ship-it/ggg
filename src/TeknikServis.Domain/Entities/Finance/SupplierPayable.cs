using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Finance;

public class SupplierPayable : BaseEntity, IAuditableEntity
{
    public int SupplierId { get; set; }
    public int? BranchId { get; set; }
    public int? PartOrderId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal PaidAmount { get; set; } = 0;
    public decimal RemainingAmount { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Supplier.Supplier Supplier { get; set; } = null!;
}
