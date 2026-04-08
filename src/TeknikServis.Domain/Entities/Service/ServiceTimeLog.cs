using TeknikServis.Domain.Common;

namespace TeknikServis.Domain.Entities.Service;

public class ServiceTimeLog : BaseEntity
{
    public int ServiceRecordId { get; set; }
    public int TechnicianId { get; set; }  // UserId
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationMinutes { get; set; }  // Hesaplanan süre
    public string? Note { get; set; }

    public ServiceRecord ServiceRecord { get; set; } = null!;
}
