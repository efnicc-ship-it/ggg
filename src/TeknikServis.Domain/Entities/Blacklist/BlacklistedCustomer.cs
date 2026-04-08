using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Blacklist;

public class BlacklistedCustomer : BaseEntity, IAuditableEntity
{
    public int CustomerId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }   // Null = kalıcı

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public Customer.Customer Customer { get; set; } = null!;
}
