using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Sale;

public class DiscountRecord : BaseEntity, IAuditableEntity
{
    public int SaleRecordId { get; set; }
    public int? CampaignId { get; set; }
    public string DiscountType { get; set; } = string.Empty; // "Percentage" | "Fixed"
    public decimal Value { get; set; }
    public decimal AppliedAmount { get; set; }
    public string? Reason { get; set; }
    public int ApprovedBy { get; set; }  // UserId

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public SaleRecord SaleRecord { get; set; } = null!;
}
