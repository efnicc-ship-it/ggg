using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Campaign;

public class Campaign : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = string.Empty; // "Percentage" | "Fixed"
    public decimal DiscountValue { get; set; }
    public decimal? MinimumPurchaseAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public int? UsageLimit { get; set; }
    public int UsageCount { get; set; } = 0;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
