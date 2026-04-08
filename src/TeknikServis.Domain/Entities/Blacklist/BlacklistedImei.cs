using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Blacklist;

public class BlacklistedImei : BaseEntity, IAuditableEntity
{
    public string Imei { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;  // "Çalıntı", "Sahte", "Yasal Engel"
    public string? Source { get; set; }                  // "Müşteri Bildirimi", "Emniyet", "Manuel"
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
