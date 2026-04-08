using Microsoft.AspNetCore.Identity;

namespace TeknikServis.Domain.Entities.Identity;

public class AppRole : IdentityRole<int>
{
    public Guid TenantId { get; set; }
    public string? Description { get; set; }
}
