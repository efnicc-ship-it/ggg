namespace TeknikServis.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
