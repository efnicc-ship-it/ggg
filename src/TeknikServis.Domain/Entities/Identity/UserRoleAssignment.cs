using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Identity;

/// <summary>
/// Bir kullanıcıya birden fazla rol atanabilir.
/// Örn: Tek kişilik işyerinde sahibi aynı anda Teknisyen + Resepsiyon + Müdür rollerini taşıyabilir.
/// </summary>
public class UserRoleAssignment : BaseEntity, IAuditableEntity
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty; // UserRole enum string veya özel rol adı
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }  // Geçici rol atamaları için (null = kalıcı)

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
