using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Tenant;

public class TenantModule
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public ModuleName ModuleName { get; set; }
    public LicenseType LicenseType { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
