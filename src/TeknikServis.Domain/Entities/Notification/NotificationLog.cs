using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Notification;

public class NotificationLog : BaseEntity
{
    public int? TemplateId { get; set; }
    public int? RecipientUserId { get; set; }
    public int? RecipientCustomerId { get; set; }
    public string? RecipientPhone { get; set; }
    public string? RecipientEmail { get; set; }
    public NotificationChannel Channel { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ProviderReference { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public bool Queued { get; set; } = false;          // Sessiz saat nedeniyle kuyruğa alındı
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
}
