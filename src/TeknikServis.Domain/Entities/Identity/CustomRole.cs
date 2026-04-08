using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Identity;

/// <summary>
/// Tenant'a özgü özel roller — sistem rolleri dışında admin tanımlayabilir.
/// Örn: "Kıdemli Teknisyen", "Depo Sorumlusu"
/// </summary>
public class CustomRole : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;       // "Kıdemli Teknisyen"
    public string? Description { get; set; }
    public string PermissionsJson { get; set; } = "[]";    // İzin listesi JSON
    public bool IsActive { get; set; } = true;
    public bool IsSystemRole { get; set; } = false;         // true = değiştirilemez

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
