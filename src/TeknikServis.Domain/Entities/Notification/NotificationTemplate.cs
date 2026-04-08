using TeknikServis.Domain.Common;
using TeknikServis.Domain.Enums;

namespace TeknikServis.Domain.Entities.Notification;

public class NotificationTemplate : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;         // "ServisAlindi", "CihazHazir"
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;         // {MüşteriAdı}, {ServisNo} gibi placeholder'lar
    public NotificationChannel Channel { get; set; }
    public string EventType { get; set; } = string.Empty;    // "ServiceReceived" | "ServiceReady" | "PriceApproval"
    public bool IsActive { get; set; } = true;
    public string? SilentHourStart { get; set; }             // "22:00"
    public string? SilentHourEnd { get; set; }               // "08:00"

    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
}
