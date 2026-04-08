using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class ServiceMedia : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;  // jpg, png, mp4
    public long FileSize { get; set; }
    public bool IsBefore { get; set; } = true;            // true: öncesi, false: sonrası
    public string? Caption { get; set; }
    public DateTime ExpiresAt { get; set; }               // CreatedAt + 2 yıl — Hangfire siler

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
