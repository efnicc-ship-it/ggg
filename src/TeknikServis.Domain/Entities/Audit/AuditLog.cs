using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Audit;

public class AuditLog : BaseEntity
{
    public new Guid TenantId { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string Action { get; set; } = string.Empty;    // "Create" | "Update" | "Delete" | "View" | "Login" | "Export"
    public string EntityType { get; set; } = string.Empty; // "ServiceRecord", "Customer"...
    public int? EntityId { get; set; }
    public string? OldValuesJson { get; set; }             // Önceki değerler (şifreli alanlar hariç)
    public string? NewValuesJson { get; set; }             // Yeni değerler
    public string? DeleteReason { get; set; }              // Silme sebebi (zorunlu)
    public bool IsSuspicious { get; set; } = false;        // Ensar AI şüpheli işaret etti
}
