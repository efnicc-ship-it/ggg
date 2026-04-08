using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Branch;

public class Branch : BaseEntity, IAuditableEntity, ISoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? RegionId { get; set; }
    public bool IsActive { get; set; } = true;

    // IAuditableEntity
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }
}
