using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class LiveStreamSchedule : BaseEntity, IAuditableEntity
{
    public int ServiceRecordId { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool NotificationSent { get; set; } = false;
    public bool IsCompleted { get; set; } = false;

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
