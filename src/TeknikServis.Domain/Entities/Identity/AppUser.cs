using Microsoft.AspNetCore.Identity;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Identity;

public class AppUser : IdentityUser<int>
{
    public Guid TenantId { get; set; }
    public int? BranchId { get; set; }
    public int? RegionId { get; set; }
    public UserRole Role { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string? ProfilePhotoPath { get; set; }
    public bool TwoFactorRequired { get; set; }

    public ICollection<UserPermission> Permissions { get; set; } = new List<UserPermission>();

    /// <summary>
    /// Birden fazla rol — tek kişilik işletmede sahibi tüm rolleri taşıyabilir.
    /// Role alanı primary/dominant rol için tutulur (geriye dönük uyumluluk + token).
    /// RoleAssignments tüm aktif rolleri içerir.
    /// </summary>
    public ICollection<UserRoleAssignment> RoleAssignments { get; set; } = new List<UserRoleAssignment>();
}
